using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public static string dScriptValidateLine(string line, out int level)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("#") || trimmed.Length == 0)
            {
                level = 0;
                return "";
            }
            string warn = "";
            int warnlevel = 0;
            if (Utilities.CountChar(line, '"') % 2 == 1)
            {
                if (warnlevel < 2) warnlevel = 2;
                warn += "Uneven number of \"quotes\" / ";
            }
            if (!(trimmed.StartsWith("-") || trimmed.Contains(":") || trimmed.StartsWith("{") || trimmed.StartsWith("}") || trimmed.StartsWith("else")))
            {
                if (warnlevel < 1) warnlevel = 1;
                warn += "Line with no clear purpose - missing a -dash- or :colon:? / ";
            }
            level = warnlevel;
            return warn;
        }
        static void dsWarn(List<string> warnings, int line, string mes, bool minor = false)
        {
            warnings.Add((minor ? "MINOR" : "WARNING") + ": Line " + line.ToString() + ": " + mes);
        }
        const int dsnomismatch = 9000001;
        static int FirstMismatch(string str, char chr)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != chr)
                {
                    return i;
                }
            }
            return dsnomismatch;
        }
        public static List<string> dsCheck(string check)
        {
            List<string> warnings = new List<string>();
            try
            {
                List<string> scrtitles = new List<string>();
                List<string> scrbasecategories = new List<string>();
                string scripttype = "";
                string totry = check.Replace("\r\n", "\n").Replace("\r", "\n") + "\nXFASSIGN:\n  type: assignment\n  actions:\n    on assignment:\n    - trigger name:click toggle:true\n  Interact Scripts:\n  - 10 taco\n";
                List<string> lines = totry.Split('\n').ToList();
                lines.Insert(0, "-- Begin YAML --");
                List<int> levels = new List<int>();
                List<bool> levelshavecats = new List<bool>();
                List<string> levelcatname = new List<string>();
                bool inscr = false;
                bool hastype = false;
                bool incat = false;
                bool cmdsfound = false;
                bool islevelchange = false;
                List<int> curlylevels = new List<int>();
                string pcatname = "";
                bool doesntneedpurpose = false;
                for (int i = 1; i < lines.Count; i++)
                {
                    if (lines[i].Length == 0)
                    {
                        continue;
                    }
                    if (lines[i].Trim().StartsWith("#"))
                    {
                        if (curlylevels.Count > 0)
                        {
                            dsWarn(warnings, i, "You can't have #comments inside an - if { .. } block!");
                        }
                        continue;
                    }
                    if (lines[i].Contains("#"))
                    {
                        dsWarn(warnings, i, "You can't use the # symbol there!");
                    }
                    if (lines[i].Contains('\t'))
                    {
                        dsWarn(warnings, i, "Tab character found (Change to spaces!)");
                        continue;
                    }
                    int clev = FirstMismatch(lines[i], ' ');
                    if (clev == dsnomismatch)
                    {
                        continue;
                    }
                    if (levels.Count == 0 && clev != 0)
                    {
                        dsWarn(warnings, i, "No script title!");
                        continue;
                    }
                    if (clev == 0)
                    {
                        switch (scripttype)
                        {
                            case "assignment":
                                if (!scrbasecategories.Contains("interact scripts"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing interact scripts category!", true);
                                }
                                if (!scrbasecategories.Contains("actions"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing actions category!", true);
                                }
                                break;
                            case "book":
                                if (!scrbasecategories.Contains("title"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing title label!");
                                }
                                if (!scrbasecategories.Contains("author"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing author label!");
                                }
                                if (!scrbasecategories.Contains("text"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing text category!");
                                }
                                break;
                            case "item":
                                if (!scrbasecategories.Contains("material"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing material label!");
                                }
                                break;
                            case "entity":
                                if (!scrbasecategories.Contains("entity_type"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing entity_type label!");
                                }
                                break;
                            case "task":
                                if (!scrbasecategories.Contains("script"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing script category!");
                                }
                                break;
                            case "procedure":
                                if (!scrbasecategories.Contains("script"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing script category!");
                                }
                                break;
                            case "world":
                                if (!scrbasecategories.Contains("events"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing events category!");
                                }
                                break;
                            case "interact":
                                if (!scrbasecategories.Contains("steps"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing steps category!");
                                }
                                if (scrbasecategories.Contains("requirements"))
                                {
                                    dsWarn(warnings, i - 1, "Script above contains a requirements section - those are outdated and should be avoided!", true);
                                }
                                break;
                            case "command":
                                if (!scrbasecategories.Contains("name"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing name label!");
                                }
                                if (!scrbasecategories.Contains("usage"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing usage label!");
                                }
                                if (!scrbasecategories.Contains("description"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing description label!");
                                }
                                if (!scrbasecategories.Contains("script"))
                                {
                                    dsWarn(warnings, i - 1, "Script above missing script section!");
                                }
                                break;
                            case "yaml data":
                                break;
                            default:
                                break;
                        }
                        scrbasecategories.Clear();
                        scripttype = "";

                        int colon = lines[i].IndexOf(':');
                        if (colon < lines[i].Length - 1)
                        {
                            if (colon < 0)
                            {
                                dsWarn(warnings, i, "Script name missing a : symbol!");
                            }
                            else
                            {
                                dsWarn(warnings, i, "Text after end of script name!");
                            }
                        }
                        scrtitles.Add(lines[i].Trim().ToLower().Replace("'", "").Replace("\"", ""));
                        for (int x = 0; x < scrtitles.Count - 1; x++)
                        {
                            if (scrtitles[scrtitles.Count - 1] == scrtitles[x])
                            {
                                dsWarn(warnings, i, "There is already a script by this name!");
                            }
                        }
                        if (scrtitles[scrtitles.Count - 1].Length < 4)
                        {
                            dsWarn(warnings, i, "This script's name should be longer to prevent conflict!");
                        }
                        levels.Clear();
                        levelshavecats.Clear();
                        levels.Add(0);
                        levelshavecats.Add(false);
                        levelcatname.Add("");
                        if (inscr)
                        {
                            if (!hastype)
                            {
                                dsWarn(warnings, i, "Script above line does not contain a TYPE value!");
                            }
                        }
                        if (incat && !cmdsfound)
                        {
                            if (pcatname != "list")
                            {
                                dsWarn(warnings, i - 1, "Category with no commands!");
                            }
                        }
                        if (curlylevels.Count > 0)
                        {
                            dsWarn(warnings, i - 1, "Unended braced { ... section - missing a }");
                            curlylevels.Clear();
                        }
                        inscr = true;
                        hastype = false;
                        incat = false;
                        cmdsfound = false;
                        doesntneedpurpose = false;
                        continue;
                    }
                    islevelchange = false;
                    if (levels[levels.Count - 1] != clev)
                    {
                        if (clev > levels[levels.Count - 1])
                        {
                            if (i == 1)
                            {
                                dsWarn(warnings, i, "Spacing on first line. No script title!");
                                continue;
                            }
                            if (lines[i - 1].Trim().StartsWith("-") && !(lines[i - 1].Trim().EndsWith("{")))
                            {
                                if (!lines[i - 1].Trim().ToLower().Replace(" ", "").Replace("^", "").StartsWith("-if"))
                                {
                                    if (levelcatname[levelcatname.Count - 1].ToLower() != "text")
                                    {
                                        dsWarn(warnings, i, "Spacing grew for no reason!");
                                    }
                                    else
                                    {
                                        doesntneedpurpose = true;
                                    }
                                }
                            }
                            cmdsfound = true;
                            levels.Add(clev);
                            levelshavecats.Add(incat);
                            levelcatname.Add(incat ? (levelcatname[levelcatname.Count - 1]) : "Nope");
                            islevelchange = true;
                        }
                        else
                        {
                            for (int f = levels.Count - 1; f >= 0; f--)
                            {
                                if (levels[f] == clev)
                                {
                                    if (curlylevels.Count > 0)
                                    {
                                        for (int x = 0; x < curlylevels.Count; x++)
                                        {
                                            if (clev < curlylevels[x])
                                            {
                                                dsWarn(warnings, i, "Spacing is shorter than the current braced {..} section!");
                                            }
                                        }
                                    }
                                    for (int g = levels.Count - 1; g > f; g--)
                                    {
                                        levels.RemoveAt(g);
                                        levelshavecats.RemoveAt(g);
                                        levelcatname.RemoveAt(g);
                                    }
                                    goto victory;
                                }
                                else if ((lines[i].Trim().StartsWith("}") || lines[i].Trim().StartsWith("else")) && levels[f] > clev)
                                {
                                    for (int g = levels.Count - 1; g > f + 1; g--)
                                    {
                                        levels.RemoveAt(g);
                                        levelshavecats.RemoveAt(g);
                                        levelcatname.RemoveAt(g);
                                    }
                                    levels.Add(clev);
                                    levelshavecats.Add(true);
                                    levelcatname.Add(levelcatname[levelcatname.Count - 1]);
                                    goto waitnodont;
                                }
                            }
                            dsWarn(warnings, i, "Bad spacing!");
                        victory:
                            islevelchange = true;
                            if (incat && !cmdsfound)
                            {
                                if (pcatname != "list")
                                {
                                    dsWarn(warnings, i - 1, "Category with no commands!");
                                }
                            }
                            incat = false;
                            if (levels.Count > 0)
                            {
                                incat = levelshavecats[levels.Count - 1];
                            }
                            cmdsfound = false;
                            doesntneedpurpose = false;
                        waitnodont:
                            ;
                        }
                    }
                    string cline = lines[i].Trim().ToLower();
                    if (levels.Count == 2)
                    {
                        if (cline.StartsWith("type:"))
                        {
                            scripttype = cline.Replace("type:", "").Trim();
                            switch (scripttype)
                            {
                                case "interact":
                                case "book":
                                case "item":
                                case "task":
                                case "assignment":
                                case "procedure":
                                case "world":
                                case "format":
                                case "inventory":
                                case "entity":
                                case "yaml data":
                                case "command":
                                case "custom":
                                    break;
                                default:
                                    dsWarn(warnings, i, "Unknown script type!");
                                    break;
                            }
                            hastype = true;
                        }
                        if (cline.Contains(':'))
                        {
                            scrbasecategories.Add(cline.Split(':')[0]);
                        }
                    }
                    if (cline.StartsWith("-"))
                    {
                        if (!incat)
                        {
                            dsWarn(warnings, i, "Script commands with no category!");
                        }
                        else
                        {
                            cmdsfound = true;
                            if ((cline.Length < 3) || (cline[1] != ' ') || (cline[2] == ' '))
                            {
                                dsWarn(warnings, i, "Commands should be written with exactly one space after the dash. Like so:     - COMMAND <arguments>");
                                continue;
                            }
                            if (incat && (levelcatname[levelcatname.Count - 1].ToLower() == "script" || ((scrbasecategories[scrbasecategories.Count - 1] == "actions" || scrbasecategories[scrbasecategories.Count - 1] == "events"))))
                            {
                                List<string> commandargs = new List<string>();
                                bool quote = false;
                                bool qtype = false;
                                int lessers = 0;
                                commandargs.Add("");
                                for (int x = 2; x < cline.Length; x++)
                                {
                                    if (cline[x] == '"')
                                    {
                                        if (quote)
                                        {
                                            if (qtype)
                                            {
                                                quote = false;
                                            }
                                        }
                                        else
                                        {
                                            quote = true;
                                            qtype = true;
                                        }
                                    }
                                    else if (cline[x] == '\'')
                                    {
                                        if (quote)
                                        {
                                            if (!qtype)
                                            {
                                                quote = false;
                                            }
                                        }
                                        else
                                        {
                                            quote = true;
                                            qtype = false;
                                        }
                                    }
                                    else if (cline[x] == ' ')
                                    {
                                        commandargs.Add("");
                                    }
                                    else if (cline[x] == '<' && x + 1 < cline.Length && cline[x + 1] != ' ' && cline[x + 1] != '=' && cline[x + 1] != '-')
                                    {
                                        lessers++;
                                    }
                                    else if (cline[x] == '>' && cline[x - 1] != ' ' && cline[x - 1] != '-')
                                    {
                                        lessers--;
                                    }
                                    else
                                    {
                                        commandargs[commandargs.Count - 1] += cline[x].ToString();
                                    }
                                }
                                if (lessers != 0)
                                {
                                    dsWarn(warnings, i, "Uneven number of <tag-marks>, are you missing a < or > ?");
                                }
                                if (quote)
                                {
                                    dsWarn(warnings, i, "Uneven number of quotes.");
                                }
                                commandargs[0] = commandargs[0].ToLower();
                                if (commandargs[0].StartsWith("^"))
                                {
                                    commandargs[0] = commandargs[0].Substring(1);
                                }
                                if (commandargs[0].StartsWith("~"))
                                {
                                    commandargs[0] = commandargs[0].Substring(1);
                                }
                                /* TODO:
                                int found = -1;
                                for (int x = 0; x < dCommands.Count; x++)
                                {
                                    if (dCommands[x].Name == commandargs[0])
                                    {
                                        found = x;
                                        break;
                                    }
                                }
                                if (found == -1)
                                {
                                    dsWarn(warnings, i, "Unknown command!");
                                }
                                else
                                {
                                    if (commandargs.Count - 1 < dCommands[found].Reqs)
                                    {
                                        dsWarn(warnings, i, "This command has " + dCommands[found].Reqs.ToString() + " required parameter" + (dCommands[found].Reqs == 1 ? "" : "s") + ". Usage: - " + dCommands[found].Info);
                                    }
                                }*/
                            }
                        }
                    }
                    else if (cline.StartsWith("{"))
                    {
                        if (cline != "{")
                        {
                            dsWarn(warnings, i, "{ should be on it's own line or at the end of the - if command line.");
                        }
                    }
                    else if (cline.StartsWith("}"))
                    {
                        if (cline != "}" && !cline.Replace(" ", "").StartsWith("}else"))
                        {
                            dsWarn(warnings, i, "} should be on it's own line.");
                        }
                    }
                    else if (cline.EndsWith(":") && !cline.StartsWith("-"))
                    {
                        if (!islevelchange)
                        {
                            if (incat && !cmdsfound)
                            {
                                if (pcatname != "list")
                                {
                                    dsWarn(warnings, i - 1, "Category with no commands!");
                                }
                            }
                            if (curlylevels.Count > 0)
                            {
                                dsWarn(warnings, i, "Category unexpected, still in an IF {..} section!");
                            }
                            if (lines[i - 1].Trim().StartsWith("-") && (clev > 2))
                            {
                                if (!levelshavecats[levels.Count - 1])
                                {
                                    dsWarn(warnings, i, "Category unexpected, in a command list!");
                                }
                            }
                        }
                        incat = true;
                        levelshavecats[levels.Count - 1] = true;
                        levelcatname[levelcatname.Count - 1] = cline.Trim().Replace(":", "").ToLower();
                        pcatname = levelcatname[levelcatname.Count - 1];
                        cmdsfound = false;
                    }
                    else
                    {
                        if (cline.Contains(":") && !cline.StartsWith("-"))
                        {
                            string pline = lines[i - 1].Trim().ToLower().Replace(" ", "").Replace("^", "").Replace("-", "");
                            if ((!cline.Contains(": ")) && !(pline.StartsWith("if") || pline.StartsWith("elseif") || cline.StartsWith("else") || pline.StartsWith("&&") || pline.StartsWith("||")))
                            {
                                dsWarn(warnings, i, "Text after category name!");
                            }
                        }
                        else
                        {
                            string pline = lines[i - 1].Trim().ToLower().Replace(" ", "").Replace("^", "").Replace("-", "");
                            if (!((pline.StartsWith("if") || pline.StartsWith("elseif") || cline.StartsWith("else") || pline.StartsWith("&&") || pline.StartsWith("||")) && !pline.Contains("{")))
                            {
                                if (!doesntneedpurpose)
                                {
                                    dsWarn(warnings, i, "Line with no clear purpose! Are you missing a - at the start, or a : at the end?");
                                }
                            }
                        }
                    }
                    if (cline.EndsWith("{"))
                    {
                        curlylevels.Add(clev);
                    }
                    if (cline.Contains("}"))
                    {
                        if (curlylevels.Count == 0)
                        {
                            dsWarn(warnings, i, "Too many }'s found!");
                        }
                        else
                        {
                            curlylevels.RemoveAt(curlylevels.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                warnings.Add("Encountered internal exception: " + ex.Message.Replace("\r", "").Replace("\n", "--") + ", please tell mcmonkey!");
            }
            return warnings;
        }
    }
}
