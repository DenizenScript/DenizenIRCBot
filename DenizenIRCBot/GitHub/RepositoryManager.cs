using DenizenIRCBot.GitHub.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DenizenIRCBot.GitHub
{
    public partial class GitHubClient
    {
        public Dictionary<string, Repository> Repositories = new Dictionary<string, Repository>();
        public Task WatchTask;
        public CancellationTokenSource TokenSource;

        public Repository GetRepository(string name)
        {
            name = name.ToLower();
            if (!Repositories.ContainsKey(name))
            {
                try
                {
                    Repositories[name] = Utilities.GetObjectFromWebResponse<Repository>(Request("repos/" + name));
                }
                catch (Exception ex)
                {
                    Logger.Output(LogType.ERROR, "Could not fetch repository '" + name + "': " + ex.Message);
                    return null;
                }
            }
            return Repositories[name];
        }

        public void WatchRepository(string name, bool hasIssues, bool hasComments, bool hasPulls)
        {
            Repository repository = GetRepository(name);
            if (repository == null)
            {
                return;
            }
            repository.GitHub = this;
            repository.HasIssues = hasIssues;
            repository.HasComments = hasComments;
            repository.HasPulls = hasPulls;
            // TODO: moar stuff!
            repository.InitEvents();

        }

        public void StartWatching()
        {
            TokenSource = new CancellationTokenSource();
            WatchTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    foreach (Repository repository in Repositories.Values)
                    {
                        try
                        {
                            repository.AnnounceEvents();
                        }
                        catch (Exception ex)
                        {
                            Logger.Output(LogType.ERROR, "Error while announcing events: " + ex.Message);
                        }
                    }
                }
            }, TokenSource.Token);
        }

        public void StopWatching()
        {
            TokenSource.Cancel();
            try
            {
                WatchTask.Wait();
            }
            catch (AggregateException ex)
            {
                Logger.Output(LogType.ERROR, "Error while stopping repo watcher task: " + ex.Message);
            }
            finally
            {
                TokenSource.Dispose();
                WatchTask = null;
            }
        }
    }
}
