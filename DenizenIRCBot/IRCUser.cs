using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DenizenIRCBot
{
    public class IRCUser
    {
        public string GetFileName()
        {
            return "data/irc_users/" + Name.ToLower().Replace('|', '_').Replace('@', '_')
                .Replace('!', '_').Replace('#', '_').Replace('$', '_').Replace('%', '_').Replace('^', '_')
                .Replace('&', '_').Replace('*', '_').Replace('(', '_').Replace(')', '_').Replace('+', '_')
                .Replace('=', '_').Replace('[', '_').Replace(']', '_').Replace('{', '_').Replace('}', '_')
                .Replace('<', '_').Replace('>', '_').Replace(',', '_').Replace('/', '_').Replace('\\', '_') + ".yml";
        }

        public IRCUser(string mask)
        {
            ParseMask(mask);
            string fileName = GetFileName();
            try
            {
                if (File.Exists(fileName))
                {
                    Settings = new YAMLConfiguration(File.ReadAllText(fileName));
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
                Settings = new YAMLConfiguration(null);
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
            string l4_time = Settings.Read("general.last_seen.4.time", null);
            if (l4_time != null)
            {
                string l4_doing = Settings.Read("general.last_seen.4.doing", null);
                Settings.Set("general.last_seen.5.time", l4_time);
                Settings.Set("general.last_seen.5.doing", l4_doing);
            }
            string l3_time = Settings.Read("general.last_seen.3.time", null);
            if (l3_time != null)
            {
                string l3_doing = Settings.Read("general.last_seen.3.doing", null);
                Settings.Set("general.last_seen.4.time", l3_time);
                Settings.Set("general.last_seen.4.doing", l3_doing);
            }
            string l2_time = Settings.Read("general.last_seen.2.time", null);
            if (l2_time != null)
            {
                string l2_doing = Settings.Read("general.last_seen.2.doing", null);
                Settings.Set("general.last_seen.3.time", l2_time);
                Settings.Set("general.last_seen.3.doing", l2_doing);
            }
            string l1_time = Settings.Read("general.last_seen.1.time", null);
            if (l1_time != null)
            {
                string l1_doing = Settings.Read("general.last_seen.1.doing", null);
                Settings.Set("general.last_seen.2.time", l1_time);
                Settings.Set("general.last_seen.2.doing", l1_doing);
            }
            Settings.Set("general.last_seen.1.time", Utilities.DateToString(DateTime.Now));
            Settings.Set("general.last_seen.1.doing", doing);
            Save();
        }

        public void Save()
        {
            string fileName = GetFileName();
            try
            {
                File.WriteAllText(fileName, Settings.SaveToString());
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
