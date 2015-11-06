using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        void SeenCommand(CommandDetails command)
        {
            if (command.Arguments.Count == 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name +" mcmonkey");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            string seen = user.GetSeen(1);
            if (seen == null)
            {
                Logger.Output(LogType.DEBUG, "Never before seen " + user.Name);
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I've never seen that user!");
                return;
            }
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I last saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
        }

        void RecentCommand(CommandDetails command)
        {
            if (command.Arguments.Count == 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name + " mcmonkey");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            string seen = user.GetSeen(1);
            if (seen == null)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I've never seen that user!");
                return;
            }
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I last saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
            for (int i = 2; i <= 5; i++)
            {
                seen = user.GetSeen(i);
                if (seen != null)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Previously, I saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
                }
            }
        }

        YAMLConfiguration IPHistory = null;

        void IPStalkCommand(CommandDetails command)
        {
            if (IPHistory == null)
            {
                IPHistory = new YAMLConfiguration(File.ReadAllText("data/iphistory.yml"));
            }
            if (command.Arguments.Count < 1)
            {
                return;
            }
            List<string> data = IPHistory.ReadStringList(getippath(command.Arguments[0]));
            if (data == null || data.Count == 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Nope, nothing for " + getippath(command.Arguments[0]));
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (string dat in data)
            {
                sb.Append(dat).Append(", ");
            }
            string dater = sb.ToString();
            dater = dater.Substring(0, dater.Length - 2);
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I've seen that IP as: " + dater, 3);
        }

        string getippath(string input)
        {
            return input.Replace(":", ".colon.".ToLower()).Replace("*", "____STAR____");
        }

        void SeenUser(string name, string ip, bool save = true)
        {
            if (ip.Contains('@'))
            {
                ip = ip.Substring(ip.IndexOf('@') + 1);
            }
            if (IPHistory == null)
            {
                if (File.Exists("data/iphistory.yml"))
                {
                    IPHistory = new YAMLConfiguration(File.ReadAllText("data/iphistory.yml"));
                }
                else
                {
                    IPHistory = new YAMLConfiguration("");
                }
            }
            List<string> paths = new List<string>();
            string basepath = getippath(ip);
            paths.Add(basepath);
            string[] split = basepath.Split('.');
            for (int i = 0; i < split.Length; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 0; x < split.Length; x++)
                {
                    if (x == i)
                    {
                        sb.Append("____STAR____");
                    }
                    else
                    {
                        sb.Append(split[x]);
                    }
                    if (x + 1 < split.Length)
                    {
                        sb.Append('.');
                    }
                }
                paths.Add(sb.ToString());
            }
            foreach (string pathy in paths)
            {
                List<object> original = IPHistory.ReadList(pathy);
                if (original == null)
                {
                    original = new List<object>();
                }
                if (!original.Contains(name.ToLower()))
                {
                    original.Add(name.ToLower());
                    IPHistory.Set(pathy, original);
                    if (save)
                    {
                        saveSeenList();
                    }
                }
            }
        }
       
        void saveSeenList()
        {
            Task.Factory.StartNew(() =>
            {
                lock (IPHistory)
                {
                    File.WriteAllText("data/iphistory.yml", IPHistory.SaveToString());
                }
            });
        }
    }
}
