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
            RateLimit = GetObjectFromResponse<RateLimit>(Request("rate_limit"));
        }

        public T GetObjectFromResponse<T>(HttpWebResponse response)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(response.GetResponseStream());
        }

        public HttpWebResponse Request(string path)
        {
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(GITHUB_URL + path);
            wr.ContentType = "application/json";
            wr.Accept = "*/*";
            wr.Method = "GET";
            wr.UserAgent = "DenizenBot";
            wr.Headers.Add("Authorization", "token " + ClientToken);
            return (HttpWebResponse)wr.GetResponse();
        }
    }
}
