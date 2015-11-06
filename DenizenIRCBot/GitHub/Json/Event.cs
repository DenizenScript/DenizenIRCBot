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

        [DataMember] string id;
        [DataMember] string type;
        [DataMember] SimplifiedUser actor;
        [DataMember] SimplifiedRepository repo;
        [DataMember] string created_at;
        [DataMember] Load payload;

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
            [DataMember] public string action;
            [DataMember] public SimplifiedIssue issue;
            [DataMember] public SimplifiedIssue pull_request;
            [DataMember] public SimplifiedComment comment;
            [DataMember(Name = "ref")] public string _ref;
            [DataMember] public List<SimplifiedCommit> commits;
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
