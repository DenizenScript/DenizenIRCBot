using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DenizenIRCBot
{
    public class IRCUser
    {
        public static Dictionary<string, YAMLConfiguration> AllConfigs = new Dictionary<string, YAMLConfiguration>();

        public static string GetFileName(string Name)
        {
            return "data/irc_users/" + Name.ToLower().Replace('|', '_').Replace('@', '_')
                .Replace('!', '_').Replace('#', '_').Replace('$', '_').Replace('%', '_').Replace('^', '_')
                .Replace('&', '_').Replace('*', '_').Replace('(', '_').Replace(')', '_').Replace('+', '_')
                .Replace('=', '_').Replace('[', '_').Replace(']', '_').Replace('{', '_').Replace('}', '_')
                .Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace('/', '_').Replace('\\', '_') + ".yml";
        }

        public string GetFileName()
        {
            return GetFileName(Name);
        }

        public static bool Exists(string username)
        {
            lock (SaveLock)
            {
                string fileName = GetFileName(username);
                if (AllConfigs.ContainsKey(fileName))
                {
                    return true;
                }
                if (File.Exists(fileName))
                {
                    return true;
                }
                return false;
            }
        }

        public IRCUser(string mask)
        {
            ParseMask(mask);
            string fileName = GetFileName();
            lock (SaveLock)
            {
                if (!AllConfigs.TryGetValue(fileName, out Settings))
                {
                    try
                    {
                        if (File.Exists(fileName))
                        {
                            Settings = new YAMLConfiguration(File.ReadAllText(fileName));
                            AllConfigs.Add(fileName, Settings);
                        }
                        else
                        {
                            Logger.Output(LogType.DEBUG, "Can't find settings for " + fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Output(LogType.ERROR, "Loading settings for " + fileName + ": " + ex.ToString());
                    }
                    if (Settings == null)
                    {
                        Settings = new YAMLConfiguration("");
                        AllConfigs.Add(fileName, Settings);
                    }
                }
            }
        }

        public void ParseMask(string mask)
        {
            if (mask.StartsWith("@"))
            {
                OP = true;
                mask = mask.Substring(1);
            }
            if (mask.StartsWith("+"))
            {
                Voice = true;
                EverHadVoice = true;
                mask = mask.Substring(1);
            }
            OriginalMask = mask;
            int s1 = mask.IndexOf('!');
            int s2 = mask.IndexOf('@');
            if (s1 > 0 && s2 > 0)
            {
                Name = mask.Substring(0, s1);
                Ident = mask.Substring(s1 + 1, (s2 - s1) - 1);
                IP = mask.Substring(s2 + 1);
            }
            else
            {
                Name = mask;
            }
        }

        public string GetSeen(int time)
        {
            string l_time = Settings.Read("general.last_seen." + time + ".time", null);
            if (l_time == null)
            {
                return null;
            }
            string l_doing = Settings.Read("general.last_seen." + time + ".doing", null);
            DateTime dt = Utilities.StringToDate(l_time).ToLocalTime();
            return Utilities.FormatDateRelative(dt) + " " + l_doing;
        }

        public void SetSeen(string doing)
        {
            SetSeen(doing, DateTime.Now);
        }

        public void SetSeen(string doing, DateTime dt)
        {
            string l5_time = Settings.Read("general.last_seen.5.time", null);
            if (l5_time != null)
            {
                DateTime l5_dt = Utilities.StringToDate(l5_time);
                if (dt.CompareTo(l5_dt) < 0)
                {
                    return;
                }
            }
            string l4_time = Settings.Read("general.last_seen.4.time", null);
            if (l4_time != null)
            {
                DateTime l4_dt = Utilities.StringToDate(l4_time);
                if (dt.CompareTo(l4_dt) < 0)
                {
                    Settings.Set("general.last_seen.5.time", Utilities.DateToString(dt));
                    Settings.Set("general.last_seen.5.doing", doing);
                    return;
                }
                string l4_doing = Settings.Read("general.last_seen.4.doing", null);
                Settings.Set("general.last_seen.5.time", l4_time);
                Settings.Set("general.last_seen.5.doing", l4_doing);
            }
            string l3_time = Settings.Read("general.last_seen.3.time", null);
            if (l3_time != null)
            {
                DateTime l3_dt = Utilities.StringToDate(l3_time);
                if (dt.CompareTo(l3_dt) < 0)
                {
                    Settings.Set("general.last_seen.4.time", Utilities.DateToString(dt));
                    Settings.Set("general.last_seen.4.doing", doing);
                    return;
                }
                string l3_doing = Settings.Read("general.last_seen.3.doing", null);
                Settings.Set("general.last_seen.4.time", l3_time);
                Settings.Set("general.last_seen.4.doing", l3_doing);
            }
            string l2_time = Settings.Read("general.last_seen.2.time", null);
            if (l2_time != null)
            {
                DateTime l2_dt = Utilities.StringToDate(l2_time);
                if (dt.CompareTo(l2_dt) < 0)
                {
                    Settings.Set("general.last_seen.3.time", Utilities.DateToString(dt));
                    Settings.Set("general.last_seen.3.doing", doing);
                    return;
                }
                string l2_doing = Settings.Read("general.last_seen.2.doing", null);
                Settings.Set("general.last_seen.3.time", l2_time);
                Settings.Set("general.last_seen.3.doing", l2_doing);
            }
            string l1_time = Settings.Read("general.last_seen.1.time", null);
            if (l1_time != null)
            {
                DateTime l1_dt = Utilities.StringToDate(l1_time);
                if (dt.CompareTo(l1_dt) < 0)
                {
                    Settings.Set("general.last_seen.2.time", Utilities.DateToString(dt));
                    Settings.Set("general.last_seen.2.doing", doing);
                    return;
                }
                string l1_doing = Settings.Read("general.last_seen.1.doing", null);
                Settings.Set("general.last_seen.2.time", l1_time);
                Settings.Set("general.last_seen.2.doing", l1_doing);
            }
            Settings.Set("general.last_seen.1.time", Utilities.DateToString(dt));
            Settings.Set("general.last_seen.1.doing", doing);
            Save();
        }

        public List<string> GetReminders()
        {
            lock (SaveLock)
            {
                List<string> messages = Settings.ReadList("general.messages");
                if (messages == null || messages.Count == 0)
                {
                    return new List<string>();
                }
                List<string> sendMessages = new List<string>();
                for (int i = 0; i < messages.Count; i++)
                {
                    string[] dat = messages[i].Split(new char[] { ':' }, 2);
                    DateTime arrival = Utilities.StringToDate(dat[0]).ToLocalTime();
                    if (arrival.CompareTo(DateTime.Now) <= 0)
                    {
                        sendMessages.Add(dat[1]);
                        messages.RemoveAt(i--);
                    }
                    else
                    {
                        Logger.Output(LogType.INFO, "Ignoring " + Utilities.FormatDateRelative(arrival) + ", ahead-of-date!");
                    }
                }
                if (sendMessages.Count > 0)
                {
                    Settings.Set("general.messages", messages.Count == 0 ? null: messages);
                    Save();
                    return sendMessages;
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public void SendReminder(string message, DateTime arriveAt)
        {
            lock (SaveLock)
            {
                List<string> messages = Settings.ReadList("general.messages");
                if (messages == null)
                {
                    messages = new List<string>();
                }
                messages.Add(Utilities.DateToString(arriveAt) + ":" + Utilities.FormatDate(DateTime.Now) + ": " + message);
                Settings.Set("general.messages", messages);
                Save();
            }
        }

        public static Object SaveLock = new Object();

        public void Save()
        {
            string fileName = GetFileName();
            try
            {
                lock (SaveLock)
                {
                    File.WriteAllText(fileName, Settings.SaveToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Saving settings for " + fileName + ": " + ex.ToString());
            }
        }

        public bool OP;

        public bool Voice;

        public string OriginalMask;

        public string Name;

        public string Ident;

        public string IP;

        public bool EverHadVoice = false;

        public YAMLConfiguration Settings = null;
    }
}
