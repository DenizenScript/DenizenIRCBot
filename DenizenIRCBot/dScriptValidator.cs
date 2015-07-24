using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                warnings.AddRange(dScriptCheckScript(scripts[i], file.GetConfigurationSection(scripts[i])));
            }
            return warnings;
        }

        public List<string> dScriptCheckScript(string scriptname, YAMLConfiguration script)
        {
            List<string> warnings = new List<string>();
            string script_type = script.Read("type", null);
            if (string.IsNullOrEmpty(script_type))
            {
                Warn(warnings, WarnType.ERROR, "Missing script type for script '" + scriptname + "'! Ignoring contents.");
                return warnings;
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
                    if (!script.HasKey(null, "script"))
                    {
                        Warn(warnings, WarnType.ERROR, "Missing SCRIPT key for script '" + scriptname + "'!");
                    }
                    // TODO: all first-level sub nodes that are lists: script checks
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
            return warnings;
        }
    }

    public enum WarnType: byte
    {
        MINOR = 1,
        WARNING = 2,
        ERROR = 3
    }
}
