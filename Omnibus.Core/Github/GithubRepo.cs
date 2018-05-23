using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnibus.Core.Github
{
    public class GithubRepo
    {
        public string Account { get; set; }
        public string RepoName { get; set; }

        public GithubRepo(string account, string repoName)
        {
            Account = account;
            RepoName = repoName;
        }

        public override string ToString()
        {
            return $"{Account}/{RepoName}";
        }

        public static GithubRepo FromRelativePath(string path)
        {
            var split = path.Split('/');

            if (split.Length != 2)
            {
                throw new ArgumentException($"Invalid repo path. Format must be \"user/repo\" for input \"{path}\"");
            }

            return new GithubRepo(split[0], split[1]);
        }

        public void Deconstruct(out string account, out string repoName)
        {
            account = Account;
            repoName = RepoName;
        }
    }
}
