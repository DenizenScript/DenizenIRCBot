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
                Data = new Dictionary<object, object>();
            }
            else
            {
                Deserializer des = new Deserializer();
                Data = des.Deserialize<Dictionary<object, object>>(new StringReader(input));
                Logger.Output(LogType.DEBUG, "YAML Config with " + Data.Keys.Count + " root keys");
            }
        }

        public YAMLConfiguration(Dictionary<object, object> datas)
        {
            Data = datas;
        }

        public Dictionary<object, object> Data;

        public bool IsList(string path)
        {
            List<object> res = ReadList(path);
            return res != null && res.Count > 0;
        }

        public List<string> ReadStringList(string path)
        {
            List<object> data = ReadList(path);
            if (data == null)
            {
                return null;
            }
            List<string> ndata = new List<string>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                ndata.Add(data[i] + "");
            }
            return ndata;
        }

        public List<object> ReadList(string path)
        {
            try
            {
                string[] data = path.Split('.');
                int i = 0;
                dynamic obj = Data;
                while (i < data.Length - 1)
                {
                    dynamic nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<object, object>))
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
                    return objs;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Caught exception while reading YAML: " + ex.ToString());
            }
            return null;
        }

        public string Read(string path, string def)
        {
            try
            {
                string[] data = path.Split('.');
                int i = 0;
                object obj = Data;
                while (i < data.Length - 1)
                {
                    // TODO: TryGetValue?
                    object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<object, object>))
                    {
                        return def;
                    }
                    obj = nobj;
                    i++;
                }
                if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
                {
                    return def;
                }
                return ((Dictionary<object, object>)obj)[data[i]].ToString();
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Caught exception while reading YAML: " + ex.ToString());
            }
            return def;
        }

        public bool HasKey(string path, string key)
        {
            return GetKeys(path).Contains(key);
        }

        public List<string> GetKeys(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    List<string> atemp = new List<string>();
                    foreach (object xtobj in Data.Keys)
                    {
                        atemp.Add(xtobj + "");
                    }
                    return atemp;
                }
                string[] data = path.Split('.');
                int i = 0;
                object obj = Data;
                while (i < data.Length - 1)
                {
                    dynamic nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<object, object>))
                    {
                        return new List<string>();
                    }
                    obj = nobj;
                    i++;
                }
                if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
                {
                    Logger.Output(LogType.DEBUG, "Missing entry for GetKeys");
                    return new List<string>();
                }
                object tobj = ((Dictionary<object, object>)obj)[data[i]];
                if (tobj is Dictionary<object, object>)
                {
                    Dictionary<object, object>.KeyCollection objs = ((Dictionary<object, object>)tobj).Keys;
                    List<string> toret = new List<string>();
                    foreach (object o in objs)
                    {
                        toret.Add(o + "");
                    }
                    return toret;
                }
                if (!(tobj is Dictionary<string, dynamic> || tobj is Dictionary<string, object>))
                {
                    Logger.Output(LogType.DEBUG, "Invalid object type for GetKeys");
                    return new List<string>();
                }
                List<string> temp = new List<string>();
                foreach (object xtobj in ((Dictionary<object, object>)tobj).Keys)
                {
                    temp.Add(xtobj + "");
                }
                return temp;
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Caught exception while reading YAML: " + ex.ToString());
            }
            return new List<string>();
        }

        public YAMLConfiguration GetConfigurationSection(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return new YAMLConfiguration(Data);
                }
                string[] data = path.Split('.');
                int i = 0;
                object obj = Data;
                while (i < data.Length - 1)
                {
                    // TODO: TryGetValue?
                    object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                    if (nobj == null || !(nobj is Dictionary<object, object>))
                    {
                        return null;
                    }
                    obj = nobj;
                    i++;
                }
                if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
                {
                    return null;
                }
                object tobj = ((Dictionary<object, object>)obj)[data[i]];
                if (!(tobj is Dictionary<object, object>))
                {
                    return null;
                }
                return new YAMLConfiguration((Dictionary<object, object>)tobj);
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Caught exception while reading YAML: " + ex.ToString());
            }
            return null;
        }

        public void Set(string path, object val)
        {
            string[] data = path.Split('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                dynamic nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<dynamic, dynamic>();
                    ((Dictionary<object, object>)obj)[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (val == null)
            {
                ((Dictionary<object, object>)obj).Remove(data[i]);
            }
            else
            {
                ((Dictionary<object, object>)obj)[data[i]] = val;
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
