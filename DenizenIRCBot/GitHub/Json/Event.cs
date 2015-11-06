using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DenizenIRCBot.GitHub.Json
{
    [DataContract]
    public class Event
    {
        public string Id
        {
            get
            {
                return id;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
        }

        public SimplifiedUser Actor
        {
            get
            {
                return actor;
            }
        }

        public SimplifiedRepository Repo
        {
            get
            {
                return repo;
            }
        }

        public string Created
        {
            get
            {
                return created_at;
            }
        }

        [DataMember] string id = null;
        [DataMember] string type = null;
        [DataMember] SimplifiedUser actor = null;
        [DataMember] SimplifiedRepository repo = null;
        [DataMember] string created_at = null;
        [DataMember] Load payload = null;

        public Load Payload
        {
            get
            {
                return payload;
            }
        }

        [DataContract]
        public class Load
        {
            // TODO: Consistency -> Make these private or others public!
            [DataMember] public string action = null;
            [DataMember] public SimplifiedIssue issue = null;
            [DataMember] public SimplifiedIssue pull_request = null;
            [DataMember] public SimplifiedComment comment = null;
            [DataMember(Name = "ref")] public string _ref = null;
            [DataMember] public List<SimplifiedCommit> commits = null;
        }

        [DataContract]
        public class SimplifiedUser
        {
            [DataMember] public string login;
            [DataMember] public string name;
        }

        [DataContract]
        public class SimplifiedRepository
        {
            [DataMember] public string name;
        }

        [DataContract]
        public class SimplifiedIssue
        {
            [DataMember] public string title;
            [DataMember] public int number;
            [DataMember] public string html_url;
        }

        [DataContract]
        public class SimplifiedComment
        {
            [DataMember] public string html_url;
        }

        [DataContract]
        public class SimplifiedCommit
        {
            [DataMember] public string message;
            [DataMember] public SimplifiedUser author;
            [DataMember] public string url;

            public string ShortUrl
            {
                get
                {
                    return Bitly.ShortenUrl(url.Replace("api.github.com/repos", "github.com").Replace("commits", "commit"));
                }
            }
        }
    }
}
