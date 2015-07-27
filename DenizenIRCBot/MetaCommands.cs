using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        bool MetaCommandIntro<T>(CommandDetails command, string type, string shorttype, List<T> aobjects, List<T> cobjects, List<T> xobjects, List<T> bobjects, bool autoSearch, bool allowList) where T : dObject
        {
            if (command.Arguments.Count < 1)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: "
                    + ColorHighlightMajor + Prefixes[0] + command.Name + " <" + type + " to find, 'all', or 'list'> <optional information request>");
                return false;
            }
            string arg = command.Arguments[0].ToLower();
            if (arg == "all")
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "A list of all " + type + "s is available at" + ColorLink + " http://mcmonkey.org/denizen/" + shorttype);
                return false;
            }
            if (arg == "list")
            {
                if (!allowList)
                {
                    return true;
                }
                else
                {
                    ListdObject(command, cobjects, xobjects, bobjects, type);
                }
                return false;
            }
            if (autoSearch)
            {
                List<T> found = new List<T>();
                foreach (T obj in aobjects)
                {
                    string bn = obj.BasicName.ToLower();
                    if (bn == arg)
                    {
                        found = new List<T>();
                        found.Add(obj);
                        break;
                    }
                    else if (bn.Contains(arg))
                    {
                        found.Add(obj);
                    }
                }
                if (found.Count == 0)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Could not find any matches.");
                }
                else if (found.Count == 1)
                {
                    showdObject(command, found[0]);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Found " + found.Count + " possible " + type + "s... ");
                    foreach (T obj in found)
                    {
                        sb.Append(obj.BasicName).Append(", ");
                    }
                    string toret = sb.ToString().Substring(0, sb.Length - 2);
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + toret);
                }
            }
            return true;
        }

        void showdObject(CommandDetails command, dObject obj)
        {
            obj.ShowToChannel(this, command);
        }

        void ListdObject<T>(CommandDetails command, List<T> cobjects, List<T> xobjects, List<T> bobjects, string objecttype) where T : dObject
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Core " + objecttype + "s: ");
            foreach (T obj in cobjects)
            {
                sb.Append(obj.BasicName).Append(' ');
            }
            int limit = Chat(command.Channel.Name, command.Pinger + ColorGeneral + sb.ToString(), 7);
            sb = new StringBuilder();
            sb.Append("Bukkit " + objecttype + "s: ");
            foreach (T obj in bobjects)
            {
                sb.Append(obj.BasicName).Append(' ');
            }
            limit = Chat(command.Channel.Name, command.Pinger + ColorGeneral + sb.ToString(), limit);
            sb = new StringBuilder();
            sb.Append("External " + objecttype + "s: ");
            foreach (T obj in xobjects)
            {
                sb.Append(obj.BasicName).Append(' ');
            }
            limit = Chat(command.Channel.Name, command.Pinger + ColorGeneral + sb.ToString(), limit);
        }
        
        void CmdCommand(CommandDetails command)
        {
            if (!MetaCommandIntro(command, "command", "cmds", AllMeta.Commands, CoreMeta.Commands, ExternalMeta.Commands, BukkitMeta.Commands, true, true))
            {
                return;
            }
        }

        void TagCommand(CommandDetails command)
        {
            if (!MetaCommandIntro(command, "tag", "tags", AllMeta.Tags, CoreMeta.Tags, ExternalMeta.Tags, BukkitMeta.Tags, true, false))
            {
                return;
            }
            // TODO: Advanced tag search
        }

        void MechCommand(CommandDetails command)
        {
            if (!MetaCommandIntro(command, "mechanism", "mecs", AllMeta.Mechanisms, CoreMeta.Mechanisms, ExternalMeta.Mechanisms, BukkitMeta.Mechanisms, true, false))
            {
                return;
            }
        }

        void LangCommand(CommandDetails command)
        {
            if (!MetaCommandIntro(command, "language explanation", "lngs", AllMeta.Languages, CoreMeta.Languages, ExternalMeta.Languages, BukkitMeta.Languages, true, true))
            {
                return;
            }
        }

        void TutCommand(CommandDetails command)
        {
            if (!MetaCommandIntro(command, "tutorial", "tuts", AllMeta.Tutorials, CoreMeta.Tutorials, ExternalMeta.Tutorials, BukkitMeta.Tutorials, true, true))
            {
                return;
            }
        }
    }
}
