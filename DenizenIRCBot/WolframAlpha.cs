using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace DenizenIRCBot
{
    public class WolframAlpha
    {
        public static readonly string WOLFRAM_URL = "http://api.wolframalpha.com/v2/";
        public static string AppID;

        public static void Init(string appid)
        {
            AppID = appid;
        }

        public static QueryResult Query(string input, string hostMask)
        {
            string url = WOLFRAM_URL + "query?input=" + Uri.EscapeDataString(input) + "&appid=" + AppID + "&ip=" + hostMask + "&format=plaintext";
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            XmlDocument doc = new XmlDocument();
            doc.Load(wr.GetResponse().GetResponseStream());
            return new QueryResult(doc);
        }

        public class QueryResult
        {
            public bool Success;
            public bool Error;
            public string SpellCheck;
            public string Input;
            public string PodID;
            public string Result;
            public string Suggestion;

            private static readonly Regex DECODING_REGEX = new Regex(@"\:(?<Value>[a-fA-F0-9]{4})", RegexOptions.Compiled);

            private static readonly string[] equals = new string[]
            {
                "Substitution", "UnitSystem", "Encodings"
            };

            private static readonly string[] startswith = new string[]
            {
                "Identification", "CityLocation", "Definition", "BasicInformation", "Taxonomy", "BasicProperties",
                "PhysicalCharacteristics", "TranslationsToEnglish", "HostInformationPodIP", "FlightStatus", "Area"
            };

            public QueryResult(XmlDocument document)
            {
                Success = document.DocumentElement.GetAttribute("success").StartsWith("t");
                Error = document.DocumentElement.GetAttribute("error").StartsWith("t");
                XmlNodeList warnings = document.DocumentElement.SelectNodes("warnings");
                if (warnings.Count > 0)
                {
                    XmlNodeList spellcheck = warnings[0].SelectNodes("spellcheck");
                    SpellCheck = spellcheck.Count > 0 ? spellcheck[0].Attributes["text"].Value.Replace("&quot;", "'") : null;
                }
                XmlNode resultPod = null;
                foreach (XmlNode pod in document.DocumentElement.SelectNodes("pod"))
                {
                    string podId = pod.Attributes["id"].Value;
                    switch (podId)
                    {
                        case "Input":
                            Input = pod.SelectSingleNode("subpod").SelectSingleNode("plaintext").InnerText;
                            break;
                        case "Result":
                            resultPod = pod;
                            break;
                        default:
                            if (checkForResult(pod))
                            {
                                PodID = podId;
                                setResult(pod);
                                return;
                            }
                            break;
                    }
                }
                if (resultPod != null)
                {
                    PodID = "Result";
                    setResult(resultPod);
                    return;
                }
                XmlNodeList futures = document.DocumentElement.SelectNodes("futuretopic");
                if (futures.Count > 0)
                {
                    XmlNode future = futures[0];
                    Result = future.Attributes["topic"].Value + " = " + future.Attributes["msg"].Value;
                    return;
                }
                XmlNodeList suggestions = document.DocumentElement.SelectNodes("didyoumeans");
                if (suggestions.Count > 0)
                {
                    XmlNodeList didyoumeans = suggestions[0].SelectNodes("didyoumean");
                    if (didyoumeans.Count > 0)
                    {
                        Suggestion = didyoumeans[0].InnerText;
                        return;
                    }
                }
            }

            private static bool checkForResult(XmlNode pod)
            {
                if (pod.Attributes["primary"] != null)
                {
                    return true;
                }
                string podId = pod.Attributes["id"].Value;
                if (equals.Contains(podId))
                {
                    return true;
                }
                if (podId.Contains(':'))
                {
                    foreach (string s in startswith)
                    {
                        if (podId.StartsWith(s))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static string DecodeUnicode(string value)
            {
                return DECODING_REGEX.Replace(value.Replace(@"\", ""), m => { return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString(); }).Replace("\n", " - ");
            }

            private void setResult(XmlNode pod)
            {
                string result = pod.SelectSingleNode("subpod").SelectSingleNode("plaintext").InnerText;
                setResult(DecodeUnicode(result));
            }

            private void setResult(string result)
            {
                if (PodID == "Substitution")
                {
                    foreach (string sub in Input.Replace("{", "").Replace("}", "").Split(','))
                    {
                        string substitution = sub.Trim();
                        if (!substitution.Contains('='))
                        {
                            continue;
                        }
                        string[] split = substitution.Split('=');
                        result.Replace(split[0].Replace(" ", ""), split[1].Replace(" ", ""));
                    }
                    if (result.Contains('='))
                    {
                        string[] split = result.Split('=');
                        Input = split[0].Trim();
                        result = result.Substring(split[0].Length);
                    }
                }
                Result = result.Trim();
            }
        }
    }
}
