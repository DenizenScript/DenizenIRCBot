using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YamlDotNet.Serialization;

namespace DenizenIRCBot
{
    public class YAMLConfiguration
    {
        public YAMLConfiguration(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Logger.Output(LogType.DEBUG, "Empty YAML config");
                Data = new Dictionary<string, dynamic>();
            }
            else
            {
                Deserializer des = new Deserializer();
                Data = des.Deserialize<Dictionary<string, dynamic>>(new StringReader(input));
                Logger.Output(LogType.DEBUG, "YAML Config with " + Data.Keys.Count + " root keys");
            }
        }

        public Dictionary<string, dynamic> Data;

        public List<string> ReadList(string path)
        {
            try
            {
                string[] data = path.Split('.');
                int i = 0;
                dynamic obj = Data;
                while (i < data.Length - 1)
                {
                    dynamic nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<string, dynamic> || nobj is Dictionary<string, object> || nobj is Dictionary<object, object>))
                    {
                        return null;
                    }
                    obj = nobj;
                    i++;
                }
                if (!obj.ContainsKey(data[i]) || !(obj[data[i]] is List<string> || obj[data[i]] is List<object>))
                {
                    return null;
                }
                if (obj[data[i]] is List<object>)
                {
                    List<object> objs = (List<object>)obj[data[i]];
                    List<string> nstr = new List<string>();
                    for (int x = 0; x < objs.Count; x++)
                    {
                        nstr.Add(objs[x] + "");
                    }
                    return nstr;
                }
                return (List<string>)obj[data[i]];
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.DEBUG, "Caught exception while reading YAML: " + ex.ToString());
            }
            return null;
        }

        public string Read(string path, string def)
        {
            try
            {
                string[] data = path.Split('.');
                int i = 0;
                dynamic obj = Data;
                while (i < data.Length - 1)
                {
                    dynamic nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<string, dynamic> || nobj is Dictionary<string, object> || nobj is Dictionary<object, object>))
                    {
                        return def;
                    }
                    obj = nobj;
                    i++;
                }
                if (!obj.ContainsKey(data[i]))
                {
                    return def;
                }
                return obj[data[i]].ToString();
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.DEBUG, "Caught exception while reading YAML: " + ex.ToString());
            }
            return def;
        }

        public void Set(string path, object val)
        {
            string[] data = path.Split('.');
            int i = 0;
            dynamic obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<string, dynamic> || nobj is Dictionary<string, object> || nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<dynamic, dynamic>();
                    obj[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (val == null)
            {
                obj.Remove(data[i]);
            }
            else
            {
                obj[data[i]] = val;
            }
        }

        public string SaveToString()
        {
            Serializer ser = new Serializer();
            StringWriter sw = new StringWriter();
            ser.Serialize(sw, Data);
            return sw.ToString();
        }
    }
}
