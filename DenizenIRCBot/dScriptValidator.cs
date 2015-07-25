using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public static string CleandScript(string input)
        {
            string[] lines = input.Replace("\t", "    ").Replace("\r", "").Split('\n');
            StringBuilder result = new StringBuilder(input.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                String line = lines[i].Trim();
                if (!line.StartsWith("#"))
                {
                    if ((line.StartsWith("}") || line.StartsWith("{") || line.StartsWith("else")) && !line.EndsWith(":"))
                    {
                        result.Append(' ');
                        result.Append(lines[i].Replace(": ", "<&co>").Replace("#", "<&ns>")).Append("\n");
                    }
                    else
                    {
                        if (!line.EndsWith(":") && line.StartsWith("-"))
                        {
                            result.Append(lines[i].Replace(": ", "<&co>").Replace("#", "<&ns>")).Append("\n");
                        }
                        else
                        {
                            result.Append(lines[i]).Append("\n");
                        }
                    }
                }
                else
                {
                    result.Append("\n");
                }
            }
            result.Append("\n");
            return result.ToString();
        }

        private void Warn(List<string> warnings, WarnType type, string warning)
        {
            warnings.Add(type.ToString() + ": " + warning);
        }

        public List<string> dScriptCheck(string rawYAML)
        {
            string fixedYAML = CleandScript(rawYAML);
            List<string> warnings = new List<string>();
            YAMLConfiguration file;
            try
            {
                file = new YAMLConfiguration(fixedYAML);
            }
            catch (Exception ex)
            {
                Warn(warnings, WarnType.ERROR, "Failed to read YAML: " + ex.Message);
                return warnings;
            }
            List<string> scripts = file.GetKeys(null);
            for (int i = 0; i < scripts.Count; i++)
            {
                dScriptCheckScript(warnings, scripts[i], file.GetConfigurationSection(scripts[i]));
            }
            return warnings;
        }

        public void dScriptCheckScript(List<String> warnings, string scriptname, YAMLConfiguration script)
        {
            string script_type = script.Read("type", null);
            if (string.IsNullOrEmpty(script_type))
            {
                Warn(warnings, WarnType.ERROR, "Missing script type for script '" + scriptname + "'! Ignoring contents.");
                return;
            }
            switch (script_type)
            {
                case "interact":
                    if (!script.HasKey(null, "steps"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing STEPS key for script '" + scriptname + "'!");
                    }
                    break;
                case "yaml data":
                    // Note: Nothing required here
                    break;
                case "entity":
                    // TODO: Entity required keys
                    break;
                case "custom":
                    // Note: Nothing required here
                    // TODO: tags: and requirements: script checks
                    break;
                case "assignment":
                    if (!script.HasKey(null, "interact scripts"))
                    {
                        Warn(warnings, WarnType.MINOR, "Missing INTERACT SCRIPTS key for script '" + scriptname + "'!");
                    }
                    if (!script.HasKey(null, "actions"))
                    {
                        Warn(warnings, WarnType.MINOR, "Missing ACTIONS key for script '" + scriptname + "'!");
                    }
                    // TODO: all actions, validate
                    break;
                case "task":
                    {
                        if (!script.HasKey(null, "script"))
                        {
                            Warn(warnings, WarnType.ERROR, "Missing SCRIPT key for script '" + scriptname + "'!");
                        }
                        List<string> keys = script.GetKeys(null);
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (script.IsList(keys[i]))
                            {
                                List<object> scriptData = script.ReadList(keys[i]);
                                ReadScriptData(warnings, scriptData, scriptname + "." + keys[i], "");
                            }
                        }
                    }
                    break;
                case "procedure":
                    if (!script.HasKey(null, "script"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing SCRIPT key for script '" + scriptname + "'!");
                    }
                    // TODO: Procedure required keys
                    break;
                case "world":
                    if (!script.HasKey(null, "events"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing EVENTS key for script '" + scriptname + "'!");
                    }
                    break;
                case "book":
                    // TODO: Book required keys
                    break;
                case "command":
                    if (!script.HasKey(null, "script"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing SCRIPT key for script '" + scriptname + "'!");
                    }
                    // TODO: Command required keys
                    break;
                case "format":
                    if (!script.HasKey(null, "format"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing FORMAT key for script '" + scriptname + "'!");
                    }
                    break;
                case "inventory":
                    // TODO: Inventory required keys
                    break;
                case "item":
                    if (!script.HasKey(null, "material"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing MATERIAL key for script '" + scriptname + "'!");
                    }
                    // TODO: Validate inputs
                    break;
                case "map":
                    // TODO: Map required keys
                    break;
                case "version":
                    // TODO: Version required keys
                    break;
                default:
                    Warn(warnings, WarnType.ERROR, "Unknown script type '" + script_type + "' for script '" + scriptname + "'!");
                    break;
            }
        }

        public void ReadScriptData(List<string> warnings, List<object> scriptData, string scriptName, string owner)
        {
            string pcommand = "";
            for (int i = 0; i < scriptData.Count; i++)
            {
                if (scriptData[i] is string)
                {
                    string cmd = (string)scriptData[i];
                    ValidateSingleCommand(warnings, cmd, pcommand, false, scriptName, owner);
                    pcommand = cmd;
                }
                else if (scriptData[i] is Dictionary<object, object>)
                {
                    Dictionary<object, object> dict = (Dictionary<object, object>)scriptData[i];
                    if (dict.Keys.Count == 1)
                    {
                        string key = dict.Keys.First().ToString();
                        ValidateSingleCommand(warnings, key, pcommand, true, scriptName, owner);
                        object value = dict.Values.First();
                        if (!(value is List<object>))
                        {
                            Warn(warnings, WarnType.WARNING, "Unrecognizable entry in sub-key for '" + key + "' for " + scriptName);
                        }
                        else
                        {
                            ReadScriptData(warnings, (List<object>)value, scriptName + "->" + key, key);
                        }
                        pcommand = key;
                    }
                    else
                    {
                        Warn(warnings, WarnType.WARNING, "Incorrect number of keys in command-set for " + scriptName);
                    }
                }
            }
        }

        public dCommand getCmd(string cmd)
        {
            foreach (dCommand command in AllMeta.Commands)
            {
                if (cmd == command.Name.ToLower())
                {
                    return command;
                }
            }
            return null;
        }

        Regex args = new Regex("[^\\s\"'¨]+|\"([^\"]*)\"|'([^']*)'|¨([^¨]*)¨"); // Note: copied from Denizen source

        public List<string> splitCommandArguments(string cmd)
        {
            List<string> res = new List<string>();
            string[] dat = cmd.Split(new char[] { ' ' }, 2);
            res.Add(dat[0]);
            if (dat.Length == 1)
            {
                return res;
            }
            foreach (Match match in args.Matches(dat[1]))
            {
                if (match.Groups[1] != null)
                {
                    res.Add(match.Groups[1].Value);
                }
                else if (match.Groups[2] != null)
                {
                    res.Add(match.Groups[2].Value);
                }
                else
                {
                    res.Add(match.Groups[0].Value);
                }
            }

            return res;
        }

        public void ValidateSingleCommand(List<string> warnings, string command, string pcommand, bool hasSubset, string scriptName, string owner)
        {
            List<string> cmddata = splitCommandArguments(command);
            string cmd = cmddata[0].ToLower();
            cmddata.RemoveAt(0);
            dCommand dcmd = getCmd(cmd);
            if ((cmd == "case" || cmd == "default") && owner.ToLower().StartsWith("choose"))
            {
                if ((cmd == "case" && cmddata.Count != 1) || (cmd == "default" && cmddata.Count != 0))
                {
                    Warn(warnings, WarnType.WARNING, "Invalid case '<<" + command + ">>' for " + scriptName);
                }
            }
            else if (dcmd == null)
            {
                Warn(warnings, WarnType.ERROR, "Unknown command '" + cmd + "' for " + scriptName);
            }
            else
            {
                if (cmddata.Count < dcmd.Reqs)
                {
                    Warn(warnings, WarnType.ERROR, "Not enough arguments in '<<" + command + ">>' (expected at least " + dcmd.Reqs + ") for " + scriptName);
                }
                byte itype = 0;
                for (int i = 0; i < command.Length; i++)
                {
                    if (itype == 0 && command[i] == '\"')
                    {
                        itype = 1;
                    }
                    else if (itype == 0 && command[i] == '\'')
                    {
                        itype = 2;
                    }
                    else if (itype == 1 && command[i] == '\"')
                    {
                        itype = 0;
                    }
                    else if (itype == 2 && command[i] == '\'')
                    {
                        itype = 0;
                    }
                }
                if (itype != 0)
                {
                    Warn(warnings, WarnType.WARNING, "Uneven quotes in '<<" + command + ">>' for " + scriptName);
                }
            }
        }
    }

    public enum WarnType: byte
    {
        MINOR = 1,
        WARNING = 2,
        ERROR = 3
    }
}
