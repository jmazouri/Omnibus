using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Omnibus.Core.Config;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.GitHub;

namespace Omnibus.Core.Github
{
    public class ReadFromGithub : IModule, IDisposable
    {
        private GithubConfig _config;
        private GitHubClient _cli = new GitHubClient(new ProductHeaderValue("Omnibus"));
        private List<GithubRepo> _repos = new List<GithubRepo>();

        private HttpClient _httpClient = new HttpClient();

        public ReadFromGithub(GithubConfig config, IEnumerable<GithubRepo> repos)
        {
            _config = config;

            Trace.Information("Logging into Github as {0}", _config.Login);

            _cli.Credentials = _config.GetCredentials();

            Trace.Information("Rate limit remaining: {0}", _cli.Miscellaneous.GetRateLimits().GetAwaiter().GetResult().Resources.Core.Remaining);

            _repos.AddRange(_config.Repos);

            Trace.Information("Using repos from config: {0}", String.Join(", ", _config.RepoPaths));

            _repos.AddRange(repos);

            Trace.Information("Using repos passed to constructor: {0}", String.Join(", ", repos));
        }

        public ReadFromGithub(GithubConfig config, params (string owner, string name)[] repos) 
            : this(config, repos.Select(d=> new GithubRepo(d.owner, d.name))) { }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var tasks = new List<Task<(RepositoryContent file, string content)>>();

            void GetDocsRecursive(string owner, string name, string path)
            {
                var results = _cli.Repository.Content.GetAllContents(owner, name, path).GetAwaiter().GetResult();

                foreach (var file in results)
                {
                    if (file.Type.Value == ContentType.Dir)
                    {
                        GetDocsRecursive(owner, name, file.Path);
                        continue;
                    }

                    tasks.Add(GetReponse(file));
                }
            }

            foreach (var (owner, name) in _repos)
            {
                GetDocsRecursive(owner, name, "/");
            }

            Task.WhenAll(tasks).Wait();

            var (faulted, succeeded) = tasks.Partition(d => d.IsFaulted);

            var result = succeeded
                .Select(d => d.Result)
                .AsParallel().Select(response =>
                {
                    string repoPath = response.file.HtmlUrl;

                    int start = repoPath.IndexOf(".com/") + 5;
                    int end = repoPath.IndexOf("/blob") - start;

                    repoPath = repoPath.Substring(start, end);

                    var dict = new Dictionary<string, object>
                        {
                            { Keys.RelativeFilePath, Path.Combine(repoPath, response.file.Path) }
                        };

                    Trace.Verbose("Read file {0}", response.file.DownloadUrl);

                    return context.GetDocument(response.file.DownloadUrl, new MemoryStream(Encoding.UTF8.GetBytes(response.content), false), dict);
                });

            foreach (var failed in faulted)
            {
                Trace.Warning("Could not retrieve file {0}", failed.Result.file.Url);
            }

            return Enumerable.Concat(result, inputs);
        }

        private async Task<(RepositoryContent file, string content)> GetReponse(RepositoryContent file)
        {
            try
            {
                return (file, await _httpClient.GetStringAsync(file.DownloadUrl));
            }
            catch (HttpRequestException ex)
            {
                Trace.Error("Could not download file {0}, error: {1}", file.DownloadUrl, ex.Message);
                return (file, null);
            }
        }
    }
}
