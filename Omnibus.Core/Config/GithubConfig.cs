using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Octokit;
using Omnibus.Core.Github;

namespace Omnibus.Core.Config
{
    public class GithubConfig
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public List<string> RepoPaths { get; set; }

        public IEnumerable<GithubRepo> Repos => RepoPaths.Select(d => GithubRepo.FromRelativePath(d));

        public Credentials GetCredentials()
        {
            if (String.IsNullOrWhiteSpace(Login))
            {
                throw new ArgumentException("Cannot be null/empty", nameof(Login));
            }

            if (String.IsNullOrWhiteSpace(Password))
            {
                throw new ArgumentException("Cannot be null/empty", nameof(Password));
            }

            return new Credentials(Login, Password);
        }
    }
}
