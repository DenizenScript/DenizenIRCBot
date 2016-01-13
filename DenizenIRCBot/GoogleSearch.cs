using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace DenizenIRCBot
{
    public class GoogleSearch
    {
        public static readonly string GOOGLE_URL = "http://ajax.googleapis.com/ajax/services/search/web?rsz=1&v=1.0&q=";

        public static Response Search(string input)
        {
            string url = GOOGLE_URL + Uri.EscapeDataString(input);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            return Utilities.GetObjectFromWebResponse<Response>((HttpWebResponse)wr.GetResponse());
        }

        [DataContract]
        public class Response
        {
            [DataMember] public Data responseData;
            [DataMember] public string responseDetails;
            [DataMember] public int responseStatus;
        }

        [DataContract]
        public class Data
        {
            [DataMember] public List<Result> results;
            [DataMember] public Cursor cursor;

            [DataContract]
            public class Cursor
            {
                [DataMember] public long estimatedResultCount;
                [DataMember] public double searchResultTime;
            }
        }

        [DataContract]
        public class Result
        {
            public string Url
            {
                get
                {
                    return Uri.UnescapeDataString(url);
                }
            }

            public string Content
            {
                get
                {
                    return content;
                }
            }

            [DataMember] string url;
            [DataMember] string content;
        }
    }
}
