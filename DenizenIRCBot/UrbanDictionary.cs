using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace DenizenIRCBot
{
    public class UrbanDictionary
    {
        public static readonly string URBAN_URL = "http://api.urbandictionary.com/v0/define?term=";

        public static UrbanResponse Search(string input)
        {
            string url = URBAN_URL + Uri.EscapeDataString(input);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            return Utilities.GetObjectFromWebResponse<UrbanResponse>((HttpWebResponse)wr.GetResponse());
        }
    }

    [DataContract]
    public class UrbanResponse
    {
        [DataMember] public string result_type;
        [DataMember] public List<UrbanDefinition> list;
    }

    [DataContract]
    public class UrbanDefinition
    {
        [DataMember] public string defid;
        [DataMember] public string word;
        [DataMember] public string author;
        [DataMember] public string definition;
        [DataMember] public string example;
    }
}
