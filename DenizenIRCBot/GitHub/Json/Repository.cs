using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DenizenIRCBot.GitHub.Json
{
    [DataContract]
    public class Repository
    {
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
                return FetchIssues ? has_issues : FetchIssues;
            }
            set
            {
                FetchIssues = value;
            }
        }

        private bool FetchIssues;

        [DataMember]
        string name;
        [DataMember]
        string full_name;
        [DataMember]
        string html_url;
        [DataMember]
        string url;
        [DataMember]
        string issues_url;
        [DataMember]
        string pulls_url;
        [DataMember]
        string commits_url;
        [DataMember]
        string events_url;
        [DataMember]
        Repository parent;
        [DataMember]
        bool fork;
        [DataMember]
        bool has_issues;
    }
}
