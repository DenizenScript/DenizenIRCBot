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
                    {
                        if (!script.HasKey(null, "entity_type"))
                        {
                            Warn(warnings, WarnType.MINOR, "Missing ENTITY_TYPE key for script '" + scriptname + "'!");
                        }
                        List<string> keys = script.GetKeys(null);
                        foreach (string key in keys)
                        {
                            if (key != "entity_type" && key != "type")
                            {
                                bool exists = false;
                                for (int i = 0; i < AllMeta.Mechanisms.Count; i++)
                                {
                                    if (AllMeta.Mechanisms[i].Objectd.ToLower() == "dentity"
                                        && AllMeta.Mechanisms[i].Name.ToLower() == key)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }
                                if (!exists)
                                {
                                    Warn(warnings, WarnType.WARNING, "Unrecognized entity mechanism '" + key + "' for " + scriptname);
                                }
                            }
                        }
                    }
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
                case "world":
                    {
                        if (!script.HasKey(null, "events"))
                        {
                            Warn(warnings, WarnType.ERROR, "Missing EVENTS key for script '" + scriptname + "'!");
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
                        keys = script.GetKeys("events");
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (script.IsList("events." + keys[i]))
                            {
                                ValidateEvent(warnings, keys[i], scriptname + ".events");
                                List<object> scriptData = script.ReadList("events." + keys[i]);
                                ReadScriptData(warnings, scriptData, scriptname + ".events." + keys[i], "");
                            }
                            else
                            {
                                Warn(warnings, WarnType.ERROR, "Invalid EVENTS sub-key '" + keys[i] + "' for " + scriptname);
                            }
                        }
                    }
                    break;
                case "book":
                    // TODO: Book required keys
                    break;
                case "command":
                    {
                        if (!script.HasKey(null, "script"))
                        {
                            Warn(warnings, WarnType.ERROR, "Missing SCRIPT key for script '" + scriptname + "'!");
                        }
                        // TODO: Command required keys
                        List<string> keys = script.GetKeys(null);
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (keys[i].ToLower() != "aliases" && script.IsList(keys[i]))
                            {
                                List<object> scriptData = script.ReadList(keys[i]);
                                ReadScriptData(warnings, scriptData, scriptname + "." + keys[i], "");
                            }
                        }
                    }
                    break;
                case "format":
                    if (!script.HasKey(null, "format"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing FORMAT key for script '" + scriptname + "'!");
                    }
                    // TODO: Validate format line
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
        
        public string StripSwitches(List<string> warnings, string evtname, string scriptName, out List<string[]> switches)
        {
            string[] dat = evtname.Split(' ');
            StringBuilder res = new StringBuilder();
            List<string[]> tswitches = new List<string[]>();
            foreach (string datum in dat)
            {
                if (!datum.Contains(':'))
                {
                    res.Append(datum).Append(' ');
                }
                else
                {
                    tswitches.Add(datum.Split(new char[] { ':' }, 2));
                }
            }
            switches = tswitches;
            return res.ToString().Trim();
        }

        public void ValidateEvent(List<string> warnings, string eventname, string scriptName)
        {
            if (!eventname.StartsWith("on"))
            {
                Warn(warnings, WarnType.ERROR, "Fully invalid event '" + eventname + "', has no 'ON' in it, for " + scriptName);
                return;
            }
            if (eventname.Contains("@"))
            {
                Warn(warnings, WarnType.ERROR, "Fully invalid event '" + eventname + "', has 'x@' object notation, for " + scriptName);
                return;
            }
            if (eventname.Contains("<"))
            {
                Warn(warnings, WarnType.ERROR, "Fully invalid event '" + eventname + "', has '<...>' tags, for " + scriptName);
                return;
            }
            dEvent matched = null;
            List<string[]> switches;
            string evtname = StripSwitches(warnings, eventname, scriptName, out switches);
            foreach (dEvent evt in AllMeta.Events)
            {
                if (evt.Regex != null)
                {
                    if (evt.Regex.IsMatch(evtname))
                    {
                        matched = evt;
                        break;
                    }
                    else
                    {
                        Logger.Output(LogType.DEBUG, "Can't match '" + evtname + "' to " + evt.Regex.ToString() + "!"); // TODO: Remove me
                    }
                }
                else
                {
                    // TODO: match somehow?
                    Logger.Output(LogType.DEBUG, "Event " + evt.Names[0] + " does not have a matcher regex!");
                }
            }
            if (matched == null)
            {
                Warn(warnings, WarnType.MINOR, "Unable to recognize event '<<" + eventname
                    + ">>' (Our system can currently only recognize a small fraction of events, so this does not yet mean much), for " + scriptName);
            }
            else
            {
                // TODO: Validate switches
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
            if(cmddata.Contains("{"))
            {
                Warn(warnings, WarnType.MINOR, "Braces as raw arguments to a command (old-style block command?) in '<<" + command + ">>' for " + scriptName);
            }
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
