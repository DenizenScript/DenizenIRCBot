using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Net;
using DenizenIRCBot.GitHub.Json;

namespace DenizenIRCBot.GitHub
{
    public partial class GitHubClient
    {

        public static readonly string GITHUB_URL = "https://api.github.com/";

        public string ClientToken;

        public RateLimit RateLimit;

        public void FetchRateLimit()
        {
            RateLimit = Utilities.GetObjectFromWebResponse<RateLimit>(Request("rate_limit"));
        }

        public HttpWebResponse Request(string path, Dictionary<string, string> headers = null)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(GITHUB_URL + path);
            wr.ContentType = "application/json";
            wr.Accept = "*/*";
            wr.Method = "GET";
            wr.UserAgent = "DenizenBot";
            wr.Headers.Add("Authorization", "token " + ClientToken);
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    wr.Headers.Add(key, headers[key]);
                }
            }
            return (HttpWebResponse)wr.GetResponse();
        }
    }
}
