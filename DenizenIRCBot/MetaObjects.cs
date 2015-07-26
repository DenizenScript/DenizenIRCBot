using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DenizenIRCBot
{
    public abstract class dObject
    {
        public string BasicName = "";

        public string FileName = "";

        public string Group = "Ungrouped";

        public string Plugin = "";

        public string Warning = "";

        public string Video = "";

        public string Deprecated = "";

        public abstract string GetName();

        public virtual void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "group":
                    Group = var.Replace('\n', ' ').Trim();
                    break;
                case "plugin":
                    Plugin = var.Replace('\n', ' ').Trim();
                    break;
                case "warning":
                    Warning = var.Replace('\n', ' ').Trim();
                    break;
                case "video":
                    Video = var.Replace('\n', ' ').Trim();
                    break;
                case "deprecated":
                    Deprecated = var.Replace('\n', ' ').Trim();
                    break;
                default:
                    Logger.Output(LogType.ERROR, "Invalid var type: " + type + " in " + FileName);
                    break;
            }
        }

        public virtual void ShowToChannel(dIRCBot bot, CommandDetails command)
        {
            if (!string.IsNullOrEmpty(Group))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: In group: " + bot.ColorHighlightMajor + Group, 1);
            }
            if (!string.IsNullOrEmpty(Plugin))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: Requires the plugin(s): " + bot.ColorHighlightMajor + Plugin, 1);
            }
            if (!string.IsNullOrEmpty(Warning))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: WARNING: " + bot.ColorHighlightMajor + Warning, 2);
            }
            if (!string.IsNullOrEmpty(Deprecated))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: DEPRECATED: " + bot.ColorHighlightMajor + Deprecated, 2);
            }
            if (!string.IsNullOrEmpty(Video))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: Video on the subject:" + bot.ColorLink + " " + Video, 1);
            }
            if (command.Arguments.Count >= 2 && command.Arguments[1].ToLower().StartsWith("f"))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + ":: Found in internal file: " + FileName, 2);
            }
        }
    }

    public class dEvent: dObject
    {
        public override string GetName()
        {
            return "event";
        }

        public List<string> Names = new List<string>();

        public List<string> Switches = new List<string>();

        public List<string> Context = new List<string>();

        public string Triggers = "";

        public Regex Regex = null;

        public List<string> Determine = new List<string>();

        public bool Cancellable = false;

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "context":
                    Context = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "triggers":
                    Triggers = var.Replace('\n', ' ').Trim();
                    break;
                case "regex":
                    Regex = new Regex(var.Replace('\n', ' ').Trim());
                    break;
                case "determine":
                    Determine = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "cancellable":
                    Cancellable = var.ToLower() == "true";
                    break;
                case "events":
                case "names":
                    Names = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    BasicName = Names[0];
                    break;
                case "switch":
                    Switches.Add(var);
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }
    }

    public class dLanguage : dObject
    {
        public override string GetName()
        {
            return "language";
        }

        public string Name = "";

        public string Description = "";

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "name":
                    Name = var.Replace('\n', ' ').Trim();
                    BasicName = Name;
                    break;
                case "description":
                    Description = var.Replace('\n', ' ').Trim();
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }
    }

    public class dCommand : dObject
    {
        public override string GetName()
        {
            return "command";
        }

        public string Name = "";

        public string Info = "";

        public List<string> Usage = new List<string>();

        public string Description = "";

        public string Author = "";

        public string Stable = "";

        public List<string> Tags = new List<string>();

        public string Short = "";

        public int Reqs = 0;

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "name":
                    Name = var.Replace('\n', ' ').Trim();
                    BasicName = Name;
                    break;
                case "info":
                case "syntax":
                    Info = var.Replace('\n', ' ').Trim();
                    break;
                case "description":
                    Description = var.Replace('\n', ' ').Trim(); // TODO: Multiline?
                    break;
                case "author":
                    Author = var.Replace('\n', ' ').Trim();
                    break;
                case "stable":
                    Stable = var.Replace('\n', ' ').Trim();
                    break;
                case "tags":
                    Tags = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "short":
                    Short = var.Replace('\n', ' ').Trim();
                    break;
                case "reqs":
                case "required":
                    Reqs = Utilities.StringToInt(var);
                    break;
                case "usage":
                    Usage.Add(var);
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }

        public override void ShowToChannel(dIRCBot bot, CommandDetails command)
        {
            bot.Chat(command.Channel.Name, command.Pinger + bot.ColorGeneral + "Found: " + bot.ColorHighlightMajor + Name + ": " + Short, 2);
            bot.Chat(command.Channel.Name, bot.ColorGeneral + "Syntax: " + bot.ColorHighlightMajor + Info, 2); // TODO Highlight correctly!
            string arg = command.Arguments.Count >= 2 ? command.Arguments[1].ToLower() : "";
            if (arg.StartsWith("a"))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + "Author: " + bot.ColorHighlightMajor + Author, 1);
            }
            if (arg.StartsWith("d"))
            {
                if (bot.Chat(command.Channel.Name, bot.ColorGeneral + "Description: " + bot.ColorHighlightMajor + Description, 3) == 0)
                {
                    bot.Chat(command.Channel.Name, bot.ColorGeneral + "And more..." + bot.ColorLink + " http://mcmonkey.org/denizen/cmds" + Name);
                }
            }
            if (arg.StartsWith("s"))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + "Stability: " + bot.ColorHighlightMajor + Stable, 1);
            }
            if (arg.StartsWith("r"))
            {
                bot.Chat(command.Channel.Name, bot.ColorGeneral + "Required Arguments: " + bot.ColorHighlightMajor + Reqs, 1);
            }
            if (arg.StartsWith("t"))
            {
                int limit = 5;
                foreach (string str in Tags)
                {
                    // TODO: Dig up tag help short description
                    limit = bot.Chat(command.Channel.Name, bot.ColorGeneral + "Tag: " + str, limit);
                }
                if (limit == 0)
                {
                    bot.Chat(command.Channel.Name, bot.ColorGeneral + "And more..." + bot.ColorLink + " http://mcmonkey.org/denizen/cmds" + Name);
                }
            }
            if (arg.StartsWith("u"))
            {
                int limit = 5;
                foreach (string str in Usage)
                {
                    foreach (string use in str.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        limit = bot.Chat(command.Channel.Name, bot.ColorGeneral + "Usage: " + use, limit);
                    }
                }
                if (limit == 0)
                {
                    bot.Chat(command.Channel.Name, bot.ColorGeneral + "And more..." + bot.ColorLink + " http://mcmonkey.org/denizen/cmds" + Name);
                }
            }
            base.ShowToChannel(bot, command);
        }
    }

    public class dTag : dObject
    {
        public override string GetName()
        {
            return "tag";
        }

        public string Name = "";

        public string Returns = "";

        public string Info = "";

        public string Alt = "";

        public string Mechanism = "";

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "name":
                case "attribute":
                    Name = var.Replace('\n', ' ').Trim();
                    Alt = TagCleanse(Name);
                    BasicName = Name;
                    break;
                case "returns":
                    Returns = var.Replace('\n', ' ').Trim();
                    break;
                case "mechanism":
                    Mechanism = var.Replace('\n', ' ').Trim();
                    break;
                case "info":
                case "description":
                    Info = var.Replace('\n', ' ').Trim();
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }

        public static string TagCleanse(string name)
        {
            string nname = "";
            bool flip = false;
            for (int f = 0; f < name.Length; f++)
            {
                if (name[f] == '[')
                {
                    flip = true;
                }
                if (!flip)
                {
                    nname += name[f].ToString();
                }
                if (name[f] == ']')
                {
                    flip = false;
                }
            }
            if (nname.Contains('@'))
            {
                nname = nname.Substring(nname.IndexOf('@') + 1);
            }
            if (nname.Contains(':'))
            {
                nname = nname.Substring(0, nname.IndexOf(':'));
            }
            return nname.ToLower();
        }
    }

    public class dMechanism : dObject
    {
        public override string GetName()
        {
            return "mechanism";
        }

        public string Objectd = "";

        public string Name = "";

        public string Input = "";

        public string Description = "";

        public List<string> Tags = new List<string>();

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "name":
                case "attribute":
                    Name = var.Replace('\n', ' ').Trim();
                    BasicName = Name;
                    break;
                case "object":
                    Objectd = var.Replace('\n', ' ').Trim();
                    break;
                case "input":
                    Input = var.Replace('\n', ' ').Trim();
                    break;
                case "tag":
                case "tags":
                    Tags = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "info":
                case "description":
                    Description = var.Replace('\n', ' ').Trim();
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }
    }

    public class dTutorial : dObject
    {
        public override string GetName()
        {
            return "tutorial";
        }

        public string Name = "";

        public string Description = "";

        public string Code = "";

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "name":
                case "title":
                    Name = var.Replace('\n', ' ').Trim();
                    BasicName = Name;
                    break;
                case "script":
                case "code":
                case "full":
                    Code = var.Replace('\n', ' ').Trim();
                    break;
                case "info":
                case "description":
                    Description = var.Replace('\n', ' ').Trim();
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }
    }

    public class dAction : dObject
    {
        public override string GetName()
        {
            return "action";
        }

        public List<string> Names = new List<string>();

        public List<string> Context = new List<string>();

        public string Triggers = "";

        public List<string> Determine = new List<string>();

        public override void ApplyVar(string type, string var)
        {
            switch (type)
            {
                case "triggers":
                    Triggers = var.Replace('\n', ' ').Trim();
                    break;
                case "determine":
                    Determine = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "name":
                case "names":
                case "action":
                case "actions":
                    Names = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    BasicName = Names[0];
                    break;
                case "context":
                    Context = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                default:
                    base.ApplyVar(type, var);
                    break;
            }
        }
    }

    public class dVideo
    {
        public string Name = "";

        public string Author = "";

        public string AuthorURL = "";

        public string Description = "";

        public string URL = "";
    }

    public class dItem
    {
        public int ID = 0;

        public int datavalue = 0;

        public List<string> names = null;
    }

    public class dMaterial
    {
        public int ID = 0;

        public int DataValue = 0;

        public string name = "";
    }
}
