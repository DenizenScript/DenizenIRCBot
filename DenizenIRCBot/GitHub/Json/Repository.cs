using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.IO;

namespace DenizenIRCBot.GitHub.Json
{
    [DataContract]
    public class Repository
    {

        public GitHubClient GitHub;

        string LastEventId;
        Dictionary<string, string> Conditional;

        Dictionary<string, List<Event.SimplifiedCommit>> WaitingCommits;

        public void InitEvents()
        {
            Conditional = new Dictionary<string, string>();
            WaitingCommits = new Dictionary<string, List<Event.SimplifiedCommit>>();
            FetchNewEvents();
        }

        public void AnnounceEvents()
        {
            foreach (Event currEvent in FetchNewEvents())
            {
                string[] strings = ParseEventInfo(currEvent);
                foreach (string message in strings)
                {
                    dIRCBot.CurrentInstance.Chat("#denizen-dev", message);
                }
            }
            AnnounceCommits();
        }

        private List<Event> FetchNewEvents()
        {
            HttpWebResponse response;
            try
            {
                response = GitHub.Request("repos/" + FullName + "/events", Conditional);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.NotModified)
                {
                    Logger.Output(LogType.ERROR, "Error while fetching events: " + ex.Message);
                }
                return new List<Event>();
            }
            Conditional["If-None-Match"] = response.GetResponseHeader("ETag");
            List<Event> events = Utilities.GetObjectFromWebResponse<List<Event>>(response);
            string BackupLast = null;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                Event currEvent = events[i];
                string id = currEvent.Id;
                BackupLast = id;
                events.RemoveAt(i);
                if (id == LastEventId)
                {
                    break;
                }
            }
            if (events.Count > 0)
            {
                LastEventId = events[0].Id;
            }
            else
            {
                LastEventId = BackupLast;
            }
            events.Reverse();
            return events;
        }

        private string[] ParseEventInfo(Event parse)
        {
            switch (parse.Type)
            {
                case "IssuesEvent":
                    {
                        if (!HasIssues)
                        {
                            return new string[0];
                        }
                        IssuesPayload payload = new IssuesPayload(parse);
                        dIRCBot b = dIRCBot.CurrentInstance;
                        return new string[]
                        {
                            b.ColorGeneral + "[" + b.ColorHighlightMinor + parse.Repo.name + b.ColorGeneral + "] "
                                    + b.ColorHighlightMajor + parse.Actor.login + b.ColorGeneral + " " + payload.Action + " an issue: "
                                    + b.ColorHighlightMajor + payload.Title + b.ColorGeneral + " (" + b.ColorHighlightMajor + payload.Number
                                    + b.ColorGeneral + ") --" + b.ColorLink  + " " + payload.ShortUrl
                        };
                    }
                case "IssueCommentEvent":
                    {
                        if (!HasIssues || !HasComments)
                        {
                            return new string[0];
                        }
                        IssuesPayload payload = new IssuesPayload(parse);
                        dIRCBot b = dIRCBot.CurrentInstance;
                        return new string[]
                        {
                            b.ColorGeneral + "[" + b.ColorHighlightMinor + parse.Repo.name + b.ColorGeneral + "] "
                                    + b.ColorHighlightMajor + parse.Actor.login + b.ColorGeneral + " commented on an issue: "
                                    + b.ColorHighlightMajor + payload.Title + b.ColorGeneral + " (" + b.ColorHighlightMajor + payload.Number
                                    + b.ColorGeneral + ") --" + b.ColorLink  + " " + payload.ShortUrl
                        };
                    }
                case "PullRequestEvent":
                    {
                        if (!HasPulls)
                        {
                            return new string[0];
                        }
                        IssuesPayload payload = new IssuesPayload(parse);
                        dIRCBot b = dIRCBot.CurrentInstance;
                        return new string[]
                        {
                            b.ColorGeneral + "[" + b.ColorHighlightMinor + parse.Repo.name + b.ColorGeneral + "] "
                                    + b.ColorHighlightMajor + parse.Actor.login + b.ColorGeneral + " " + payload.Action + " a pull request: "
                                    + b.ColorHighlightMajor + payload.Title + b.ColorGeneral + " (" + b.ColorHighlightMajor + payload.Number
                                    + b.ColorGeneral + ") --" + b.ColorLink  + " " + payload.ShortUrl
                        };
                    }
                case "PullRequestReviewCommentEvent":
                    {
                        if (!HasPulls || !HasComments)
                        {
                            return new string[0];
                        }
                        IssuesPayload payload = new IssuesPayload(parse);
                        dIRCBot b = dIRCBot.CurrentInstance;
                        return new string[]
                        {
                            b.ColorGeneral + "[" + b.ColorHighlightMinor + parse.Repo.name + b.ColorGeneral + "] "
                                    + b.ColorHighlightMajor + parse.Actor.login + b.ColorGeneral + " commented on a pull request: "
                                    + b.ColorHighlightMajor + payload.Title + b.ColorGeneral + " (" + b.ColorHighlightMajor + payload.Number
                                    + b.ColorGeneral + ") --" + b.ColorLink  + " " + payload.ShortUrl
                        };
                    }
                case "CommitCommentEvent":
                    {
                        if (!HasComments)
                        {
                            return new string[0];
                        }
                        CommitCommentPayload payload = new CommitCommentPayload(parse);
                        dIRCBot b = dIRCBot.CurrentInstance;
                        return new string[]
                        {
                            b.ColorGeneral + "[" + b.ColorHighlightMinor + parse.Repo.name + b.ColorGeneral + "] "
                                    + b.ColorHighlightMajor + parse.Actor.login + b.ColorGeneral + " commented on a commit: "
                                    + b.ColorHighlightMajor + payload.CommitId + b.ColorGeneral + " --" + b.ColorLink  + " " + payload.ShortUrl
                        };
                    }
                case "PushEvent":
                    {
                        PushEventPayload payload = new PushEventPayload(parse);
                        string Ref = payload.Ref;
                        if (!WaitingCommits.ContainsKey(Ref))
                        {
                            WaitingCommits[Ref] = new List<Event.SimplifiedCommit>();
                        }
                        WaitingCommits[Ref].AddRange(payload.Commits);
                        return new string[0];
                    }
                default:
                    {
                        return new string[0];
                    }
            }
        }

        private void AnnounceCommits()
        {
            dIRCBot b = dIRCBot.CurrentInstance;
            foreach (string Ref in WaitingCommits.Keys)
            {
                List<string> authors = new List<string>();
                List<Event.SimplifiedCommit> commits = WaitingCommits[Ref];
                string[] lines = new string[commits.Count + 1];
                int l = 1;
                foreach (Event.SimplifiedCommit commit in commits)
                {
                    string author = commit.author.name;
                    lines[l] = "  " + b.ColorHighlightMajor + author + b.ColorGeneral + ": " + commit.message.Replace("\n", " - ")
                        + " --" + b.ColorLink + " " + commit.ShortUrl;
                    if (!author.Contains(author))
                    {
                        authors.Add(author);
                    }
                    l++;
                }
                string authorMsg = authors[0] + (authors.Count > 2 ? " and " + (authors.Count - 1) + " others"
                    : authors.Count > 1 ? " and " + authors[1] : "");
                lines[0] = b.ColorGeneral + "[" + b.ColorHighlightMinor + FullName + b.ColorGeneral + "] "
                    + b.ColorHighlightMajor + authorMsg + b.ColorGeneral + " pushed " + commits.Count + " commits to '"
                    + b.ColorHighlightMinor + Ref + "' branch";
                foreach (string line in lines)
                {
                    b.Chat("#denizen-dev", line);
                }
            }
            WaitingCommits.Clear();
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string FullName
        {
            get
            {
                return full_name;
            }
        }

        public string HtmlUrl
        {
            get
            {
                return html_url;
            }
        }

        public string Url
        {
            get
            {
                return url;
            }
        }

        public string IssuesUrl
        {
            get
            {
                return issues_url;
            }
        }

        public string PullsUrl
        {
            get
            {
                return pulls_url;
            }
        }

        public string CommitsUrl
        {
            get
            {
                return commits_url;
            }
        }

        public string EventsUrl
        {
            get
            {
                return events_url;
            }
        }

        public Repository Parent
        {
            get
            {
                return parent;
            }
        }

        public bool IsFork
        {
            get
            {
                return fork;
            }
        }

        public bool HasIssues
        {
            get
            {
                return FetchIssues ? has_issues : false;
            }
            set
            {
                FetchIssues = value;
            }
        }

        public bool HasComments
        {
            get
            {
                return FetchComments;
            }
            set
            {
                FetchComments = value;
            }
        }

        public bool HasPulls
        {
            get
            {
                return FetchPulls;
            }
            set
            {
                FetchPulls = value;
            }
        }

        private bool FetchIssues;
        private bool FetchComments;
        private bool FetchPulls;

        [DataMember] string name;
        [DataMember] string full_name;
        [DataMember] string html_url;
        [DataMember] string url;
        [DataMember] string issues_url;
        [DataMember] string pulls_url;
        [DataMember] string commits_url;
        [DataMember] string events_url;
        [DataMember] Repository parent;
        [DataMember] bool fork;
        [DataMember] bool has_issues;
    }
}
