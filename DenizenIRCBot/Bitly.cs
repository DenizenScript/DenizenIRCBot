using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace DenizenIRCBot
{
    public class Bitly
    {
        private static string BITLY_API = "https://api-ssl.bitly.com/v3/shorten?";
        private static string Main;
        private static string Backup;
        private static Dictionary<string, string> Cache;

        public static void Init(string main, string backup)
        {
            Main = main;
            Backup = backup;
            Cache = new Dictionary<string, string>();
        }

        public static string ShortenUrl(string url)
        {
            if (Cache.ContainsKey(url))
            {
                return Cache[url];
            }
            HttpWebResponse response;
            try
            {
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(BITLY_API + "login=spazzmatic&apiKey=" + Main + "&longUrl=" + url);
                wr.ContentType = "application/json";
                wr.Accept = "*/*";
                wr.Method = "GET";
                wr.UserAgent = "DenizenBot";
                response = (HttpWebResponse)wr.GetResponse();
            }
            catch (WebException)
            {
                try
                {
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(BITLY_API + "login=morphan1&apiKey=" + Backup + "&longUrl=" + url);
                    wr.ContentType = "application/json";
                    wr.Accept = "*/*";
                    wr.Method = "GET";
                    wr.UserAgent = "DenizenBot";
                    response = (HttpWebResponse)wr.GetResponse();
                }
                catch (Exception ex)
                {
                    Logger.Output(LogType.ERROR, "Error getting shortened URL from bit.ly: " + ex.Message);
                    return "";
                }
            }
            BitlyResponse br = Utilities.GetObjectFromWebResponse<BitlyResponse>(response);
            string shortened = br.data.url;
            Cache[url] = shortened;
            return shortened;
        }

        [DataContract]
        private class BitlyResponse
        {
            [DataMember] public Data data = null;

            [DataContract]
            public class Data
            {
                [DataMember] public string url = null;
            }
        }
    }
}
