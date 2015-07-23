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
                return obj[data[i]].ToString();
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.DEBUG, "Caught exception while reading YAML: " + ex.ToString());
            }
            return def;
        }

        public void Set(string path, string val)
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
            obj[data[i]] = val;
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
