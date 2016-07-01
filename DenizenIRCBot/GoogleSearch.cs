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
        public static readonly string GOOGLE_URL = "https://www.googleapis.com/customsearch/v1?cx=003567170677611767604%3At5ozk0b_f54&key=AIzaSyBDFMRuDix-v6KcvU6Hp9qw2dcTcU549Oo&q=";

        public static Response Search(string input)
        {
            string url = GOOGLE_URL + Uri.EscapeDataString(input);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            return Utilities.GetObjectFromWebResponse<Response>((HttpWebResponse)wr.GetResponse());
        }

        [DataContract]
        public class Response
        {
            [DataMember] public List<ResponseItem> items;
            [DataMember] public SearchInfo searchInformation;
        }

        [DataContract]
        public class ResponseItem
        {
            [DataMember] public string title;
            [DataMember] public string link;
            [DataMember] public string snippet;
        }

        [DataContract]
        public class SearchInfo
        {
            [DataMember] public double searchTime;
            [DataMember] public string formattedSearchTime;
            [DataMember] public long totalResults;
            [DataMember] public string formattedTotalResults;
        }
    }
}
