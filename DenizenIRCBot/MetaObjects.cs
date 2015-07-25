using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public abstract class dObject
    {
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
                    Group = var.Replace('\n', ' ');
                    break;
                case "plugin":
                    Plugin = var.Replace('\n', ' ');
                    break;
                case "warning":
                    Warning = var.Replace('\n', ' ');
                    break;
                case "video":
                    Video = var.Replace('\n', ' ');
                    break;
                case "deprecated":
                    Deprecated = var.Replace('\n', ' ');
                    break;
                default:
                    Logger.Output(LogType.ERROR, "Invalid var type: " + type);
                    break;
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

        public string Regex = "";

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
                    Triggers = var.Replace('\n', ' ');
                    break;
                case "regex":
                    Regex = var.Replace('\n', ' ');
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
                    Name = var.Replace('\n', ' ');
                    break;
                case "description":
                    Description = var.Replace('\n', ' ');
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
                    Name = var.Replace('\n', ' ');
                    break;
                case "info":
                case "syntax":
                    Info = var.Replace('\n', ' ');
                    break;
                case "description":
                    Description = var.Replace('\n', ' ');
                    break;
                case "author":
                    Author = var.Replace('\n', ' ');
                    break;
                case "stable":
                    Stable = var.Replace('\n', ' ');
                    break;
                case "tags":
                    Tags = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "short":
                    Short = var.Replace('\n', ' ');
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
                    Name = var.Replace('\n', ' ');
                    Alt = TagCleanse(Name);
                    break;
                case "returns":
                    Returns = var.Replace('\n', ' ');
                    break;
                case "mechanism":
                    Mechanism = var.Replace('\n', ' ');
                    break;
                case "info":
                case "description":
                    Info = var.Replace('\n', ' ');
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
                    Name = var.Replace('\n', ' ');
                    break;
                case "object":
                    Objectd = var.Replace('\n', ' ');
                    break;
                case "tag":
                case "tags":
                    Tags = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "info":
                case "description":
                    Description = var.Replace('\n', ' ');
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
                case "attribute":
                    Name = var.Replace('\n', ' ');
                    break;
                case "script":
                case "code":
                case "full":
                    Code = var.Replace('\n', ' ');
                    break;
                case "info":
                case "description":
                    Description = var.Replace('\n', ' ');
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
                    Triggers = var.Replace('\n', ' ');
                    break;
                case "determine":
                    Determine = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    break;
                case "name":
                case "names":
                case "action":
                case "actions":
                    Names = var.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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
