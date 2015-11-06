using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DenizenIRCBot.GitHub;
using DenizenIRCBot.GitHub.Json;
using System.IO;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public GitHubClient GitHub;
        public YAMLConfiguration GitHubConfig;

        void InitGitHub()
        {
            try
            {
                GitHub = new GitHubClient() { ClientToken = Configuration.Read("dircbot.github.token", "") };
                GitHubConfig = new YAMLConfiguration(File.ReadAllText("data/repositories.yml"));
                GitHub.FetchRateLimit();
                foreach (string author in GitHubConfig.GetKeys(null))
                {
                    foreach (string repository in GitHubConfig.GetKeys(author))
                    {
                        YAMLConfiguration repoConfig = GitHubConfig.GetConfigurationSection(author + "." + repository);
                        bool hasIssues = repoConfig.Read("has_issues", "false").StartsWith("t");
                        bool hasComments = repoConfig.Read("has_comments", "false").StartsWith("t");
                        bool hasPulls = repoConfig.Read("has_pulls", "false").StartsWith("t");
                        GitHub.WatchRepository(author + "/" + repository, hasIssues, hasComments, hasPulls);
                    }
                }
                GitHub.StartWatching();
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Failed to initialize GitHubEngine: " + ex.Message);
            }
        }
        
        void RateLimitCommand(CommandDetails command)
        {
            GitHub.FetchRateLimit();
            Notice(command.User.Name, ColorGeneral + "Max rate limit: " + GitHub.RateLimit.Limit);
            Notice(command.User.Name, ColorGeneral + "Remaining rate limit: " + GitHub.RateLimit.Remaining);
            TimeSpan span = GitHub.RateLimit.NextReset.Subtract(DateTime.UtcNow);
            int minutes = span.Minutes;
            Notice(command.User.Name, ColorGeneral + "Next reset: "
                + (minutes > 0 ? minutes + " minutes, " : "")
                + span.Seconds + " seconds");
        }

        void GitHubCommand(CommandDetails command)
        {
            if (command.Arguments.Count < 2 || command.Arguments[0].ToLower() != "info")
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written " + ColorHighlightMajor + Prefixes[0] + command.Name + " [info <repo>]");
                return;
            }
            string name = command.Arguments[1].ToLower();
            if (GitHub.Repositories.ContainsKey(name))
            {
                Repository repo = GitHub.Repositories[name];
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + repo.HtmlUrl + " (" + repo.FullName + "): Issues(" + repo.HasIssues + ")");
            }
            else
            {
                int minDist = int.MaxValue;
                string closest = "";
                if (name.Contains('/'))
                {
                    foreach (string repoName in GitHub.Repositories.Keys)
                    {
                        int distance = Utilities.GetLevenshteinDistance(name, repoName);
                        if (minDist > distance)
                        {
                            minDist = distance;
                            closest = repoName;
                        }
                    }
                }
                else
                {
                    foreach (Repository repository in GitHub.Repositories.Values)
                    {
                        int distance = Utilities.GetLevenshteinDistance(name, repository.Name.ToLower());
                        if (minDist > distance)
                        {
                            minDist = distance;
                            closest = repository.FullName;
                        }
                    }
                }
                Repository repo = GitHub.Repositories[closest.ToLower()];
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + repo.HtmlUrl + " (" + repo.FullName + "): Issues(" + repo.HasIssues + ")");
            }
        }
    }
}
