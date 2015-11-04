using DenizenIRCBot.GitHub.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot.GitHub
{
    public partial class GitHubClient
    {
        public Dictionary<string, Repository> Repositories = new Dictionary<string, Repository>();

        public Repository GetRepository(string name)
        {
            name = name.ToLower();
            if (!Repositories.ContainsKey(name))
            {
                Repositories[name] = GetObjectFromResponse<Repository>(Request("repos/" + name));
            }
            return Repositories[name];
        }

        public void WatchRepository(string name)
        {
            GetRepository(name);
            // TODO: stuff!
        }
    }
}
