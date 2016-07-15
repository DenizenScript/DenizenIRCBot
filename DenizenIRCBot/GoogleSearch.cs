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

        public static GoogleResponse Search(string input)
        {
            string url = GOOGLE_URL + Uri.EscapeDataString(input);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            return Utilities.GetObjectFromWebResponse<GoogleResponse>((HttpWebResponse)wr.GetResponse());
        }
    }

    [DataContract]
    public class GoogleResponse
    {
        [DataMember] public List<GoogleResponseItem> items;
        [DataMember] public GoogleSearchInfo searchInformation;
    }

    [DataContract]
    public class GoogleResponseItem
    {
        [DataMember] public string title;
        [DataMember] public string link;
        [DataMember] public string snippet;
    }

    [DataContract]
    public class GoogleSearchInfo
    {
        [DataMember] public double searchTime;
        [DataMember] public string formattedSearchTime;
        [DataMember] public long totalResults;
        [DataMember] public string formattedTotalResults;
    }
}
