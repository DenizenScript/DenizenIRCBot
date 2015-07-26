using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public void TryCommand(CommandDetails command)
        {
            switch (command.Name.ToLower())
            {
                case "hello":
                case "allo":
                case "ello":
                case "hi":
                case "ohai":
                case "helo":
                case "halo":
                case "hai":
                case "howdy":
                case "website":
                case "web":
                case "site":
                case "link":
                case "url":
                case "greet":
                    {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral +
                        "Hello! I am a bot designed to assist with Denizen Scripting! I have a website too, at" +
                        ColorLink + " http://mcmonkey.org/logs " + ColorGeneral + "!");
                    }
                    break;
                case "delay":
                    {
                        int delay = Utilities.random.Next(9) + 1;
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral +
                            "Delaying for " + ColorHighlightMajor + delay + ColorGeneral + " seconds!");
                        Thread.Sleep(1000 * delay);
                        Chat(command.Channel.Name, ColorGeneral + "Done delaying for " + ColorHighlightMajor + delay);
                    }
                    break;
                case "regex":
                    // TODO
                    break;
                case "regexvalue":
                    // TODO
                    break;
                case "bs":
                case "botsnack":
                case "snack":
                    BotsnackCommand(command);
                    break;
                case "lasers":
                case "lzrbms":
                case "laserbeams":
                    LaserbeamsCommand(command);
                    break;
                case "fire":
                case "fiyar":
                case "fiar":
                    FireCommand(command);
                    break;
                case "seen":
                    SeenCommand(command);
                    break;
                case "recent":
                case "recentseen":
                case "seenrecently":
                case "recently":
                case "recentlyseen":
                    RecentCommand(command);
                    break;
                case "msg":
                case "send":
                case "tell":
                case "mail":
                case "message":
                    MessageCommand(command);
                    break;
                case "remind":
                case "rem":
                case "remember":
                case "tellmelater":
                case "delayedmsg":
                case "delayedmessage":
                    ReminderCommand(command);
                    break;
                case "ts":
                case "ts3":
                case "teamspeak":
                case "teamspeak3":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, command.Pinger + "Denizen users are all welcome to voice chat together at the TeamSpeak3 server ts3.mcmonkey.org in the channel 'Denizen'!");
                    }
                    else
                    {
                        Chat(command.Channel.Name, command.Pinger + "All bot users are all welcome to voice chat together at the TeamSpeak3 server ts3.mcmonkey.org!");
                    }
                    break;
                case "please":
                case "pls":
                case "plz":
                case "plox":
                case "pl0x":
                case "sir":
                case "sir,":
                case "sir:":
                case "do":
                case "say":
                    {
                        if (command.Arguments.Count > 0)
                        {
                            CommandDetails cmddet = new CommandDetails();
                            cmddet.Name = command.Arguments[0].ToLower();
                            cmddet.Arguments = new List<string>(command.Arguments);
                            cmddet.Arguments.RemoveAt(0);
                            cmddet.Pinger = command.Pinger;
                            cmddet.User = command.User;
                            cmddet.Channel= command.Channel;
                            TryCommand(cmddet);
                        }
                    }
                    break;
                case "command":
                case "cmd":
                case "commands":
                case "cmds":
                case "c":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        CmdCommand(command);
                    }
                    break;
                case "evt":
                case "evts":
                case "event":
                case "events":
                case "e":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "mec":
                case "mecs":
                case "mech":
                case "mechs":
                case "mechanism":
                case "mechanisms":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "language":
                case "languages":
                case "info":
                case "infos":
                case "explanation":
                case "explanations":
                case "lng":
                case "lngs":
                case "text":
                case "texts":
                case "helps":
                case "lang":
                case "langs":
                case "lingo":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "tutorial":
                case "tut":
                case "tutorials":
                case "tuts":
                case "example":
                case "examples":
                case "ex":
                case "exs":
                case "exam":
                case "exams":
                case "examp":
                case "examps":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "tag":
                case "tags":
                case "t":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "vid":
                case "vids":
                case "v":
                case "video":
                case "videos":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "act":
                case "acts":
                case "action":
                case "actions":
                case "a":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "item":
                case "items":
                case "itm":
                case "itms":
                case "mat":
                case "material":
                case "mats":
                case "materials":
                case "i":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "m":
                case "math":
                case "maths":
                case "mathematic":
                case "mathematics":
                case "calculate":
                case "calc":
                    // TODO
                    break;
                case "repository":
                case "repo":
                case "repos":
                case "repositories":
                case "scripts":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Check out scripts made by other users -" + ColorLink + " " + "http://mcmonkey.org/denizen/repo/list");
                        Chat(command.Channel.Name, ColorGeneral + "Old repo -" + ColorLink + " http://bit.ly/19lCpfV");
                    }
                    break;
                case "issue":
                case "issues":
                case "iss":
                    {
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Post Denizen issues here:" + ColorLink + " https://github.com/DenizenScript/Denizen-For-Bukkit/issues");
                        }
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".citizens_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Post Denizen issues here:" + ColorLink + " https://github.com/CitizensDev/Citizens2/issues");
                        }
                    }
                    break;
                case "enchant":
                case "enchantment":
                case "enchants":
                case "enchantments":
                case "ench":
                case "enches":
                case "enchs":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral +
                            "A list of all valid bukkit enchantments is available here:" + ColorLink +
                            " https://hub.spigotmc.org/javadocs/spigot/org/bukkit/enchantments/Enchantment.html");
                        Chat(command.Channel.Name, ColorGeneral + "They do not follow the same naming conventions as they do in game, so be careful.");
                    }
                    break;
                case "entity":
                case "entitys":
                case "entities":
                case "entitie":
                case "entitytype":
                case "entitytypes":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "A list of all Entity types is available here:" + ColorLink + " http://bit.ly/1sos3dT");
                    }
                    break;
                case "thank":
                case "thanks":
                case "thanks!":
                case "thanks.":
                case "thankyou!":
                case "thankyou.":
                case "thankyou":
                case "donate":
                case "don":
                    {
                        Chat(command.Channel.Name, ColorGeneral + "Donate to fullwall (Head of the Citizens project) at:" + ColorLink + " http://bit.ly/19UVDtp");
                        Chat(command.Channel.Name, ColorGeneral + "Donate to mcmonkey (Current head of Denizen development) at:" + ColorLink + " " + "http://mcmonkey.org/donate");
                    }
                    break;
                case "potion":
                case "potions":
                case "effect":
                case "effects":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "A list of all Bukkit Potion Effects is available here:" + ColorLink + " http://bit.ly/1LnDvvv");
                    }
                    break;
                case "test":
                case "tet":
                case "tes":
                case "tst":
                case "?":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "All systems functional!");
                    }
                    break;
                case "help":
                case "halp":
                case "hlp":
                case "hep":
                case "hap":
                    {
                        string prefix = Prefixes[0];
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Hello, I am DenizenBot, a bot dedicated to assisting you in maximizing your Denizen scripting potential."
                                                                    + "   If you're new to Denizen, type " + ColorHighlightMinor + prefix + "getstarted");
                            Chat(command.Channel.Name, ColorGeneral + "To check your script for errors, type " + ColorHighlightMinor + prefix + "script");
                            Chat(command.Channel.Name, ColorGeneral + "Available meta search commands: " + ColorHighlightMinor + prefix + "cmds " + prefix + "tags " + prefix + "events "
                                                                            + prefix + "requirements " + prefix + "languages "
                                                                            + prefix + "tutorials " + prefix + "mechanisms " + prefix + "actions " + prefix + "items " + prefix + "skins " + prefix + "search "
                                                                            + prefix + "videos");
                            Chat(command.Channel.Name, ColorGeneral + "Available informational commands: " + ColorHighlightMinor + prefix + "repo " + prefix + "enchantments " + prefix + "entities " + prefix + "anchors "
                                                                            + prefix + "tags " + prefix + "potions " + prefix + "assignments " + prefix + "update " + prefix + "newconfig "
                                                                            + prefix + "wiki " + prefix + "sounds " + prefix + "handbook " + prefix + "debug " + prefix + "mcve ");
                        }
                        Chat(command.Channel.Name, ColorGeneral + "Available interactive commands: " + ColorHighlightMinor + prefix + "seen " + prefix + "message " + prefix + "hello "
                                                                        + prefix + "showoff " + prefix + "math " + prefix + "mcping "
                                                                        + prefix + "help " + prefix + "paste " + prefix + "logs " + prefix + "yaml " + prefix + "yes " + prefix + "reload " + prefix + "voice "
                                                                        + prefix + "quote " + prefix + "savelog " + prefix + "botsnack " + prefix + "regex " + prefix + "regexvalue ");
                    }
                    break;
                case "skins":
                case "skin":
                case "skinssearch":
                case "skinsearch":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".citizens_meta", "false").StartsWith("t"))
                    {
                        if (command.Arguments.Count > 1)
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "A list of matching skins for your NPCs is at:" + ColorLink + " " + "http://mcmonkey.org/denizen/skin/" + command.Arguments[1]);
                        }
                        else
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Your can search for skins for your NPCs at:" + ColorLink + " " + "http://mcmonkey.org/denizen/skin");
                        }
                    }
                    break;
                case "hastebin":
                case "haste":
                case "hastee":
                case "hastie":
                case "hastey":
                case "hastes":
                case "hastebins":
                case "pastebin":
                case "pastebins":
                case "pastey":
                case "pastests":
                case "pastie":
                case "pastee":
                case "paste":
                    // TODO
                    break;
                case "debug":
                case "db":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Need help with a script issue or server error? "
                        + "- Help us help you by pasting your script to" + ColorLink + " " + "http://mcmonkey.org/haste "
                        + ColorGeneral + "- From there, save the page and paste the link back in this channel.");
                        Chat(command.Channel.Name, ColorGeneral + "In-game, type '" + ColorHighlightMajor + "/denizen debug -r" + ColorGeneral
                            + "', then run through the broken parts of the script, then type '" + ColorHighlightMajor +
                            "/denizen submit" + ColorGeneral + "'. Open the link it gives you, and paste that link back in this channel as well.");
                        Chat(command.Channel.Name, ColorGeneral + "For more information on how to read debug, see " + ColorLink + " http://mcmonkey.org/denizen/vids/debug");
                    }
                    break;
                case "update":
                case "latest":
                case "new":
                case "newest":
                case "build":
                case "builds":
                case "spigot":
                case "spig":
                case "sp":
                    {
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Due to the nature of our project, Denizen is always built against the "
                                + ColorHighlightMajor + "development" + ColorGeneral + " builds of Spigot and Citizens."
                                + " Most errors can be fixed by updating them all.");
                        }
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, ColorGeneral + S_BOLD + "Denizen-" + S_NORMAL + ColorLink + " http://ci.citizensnpcs.co/job/Denizen/lastSuccessfulBuild");
                            Chat(command.Channel.Name, ColorGeneral + S_BOLD + "Denizen (Developmental)-" + S_NORMAL + ColorLink + " http://ci.mineconomy.org/job/Denizen_Developmental/lastSuccessfulBuild/");
                        }
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".citizens_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, ColorGeneral + S_BOLD + "Citizens-" + S_NORMAL + ColorLink + " http://ci.citizensnpcs.co/job/Citizens2/lastSuccessfulBuild");
                        }
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, ColorGeneral + S_BOLD + "Spigot-" + S_NORMAL + ColorLink + " http://bit.ly/15RZsn6");
                            Chat(command.Channel.Name, ColorGeneral + S_BOLD + "Depenizen (Optional)- " + S_NORMAL + ColorLink + " http://ci.citizensnpcs.co/job/Depenizen/lastSuccessfulBuild/");
                        }
                    }
                    break;
                case "nc":
                case "newconfig":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "If you are having issues or are unable to find a setting, you may be using the old config file.");
                        Chat(command.Channel.Name, ColorGeneral + "You can easily generate a new one by deleteing your current config.yml file in the Denizen folder.");
                    }
                    break;
                case "wiki":
                    {
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".citizens_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Citizens Wiki:" + ColorLink + " http://wiki.citizensnpcs.co");
                        }
                        if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                        {
                            Chat(command.Channel.Name, ColorGeneral + "Denizen Wiki:" + ColorLink + " http://wiki.citizensnpcs.co/Denizen");
                            Chat(command.Channel.Name, ColorGeneral + "Depenizen Wiki:" + ColorLink + " http://wiki.citizensnpcs.co/Depenizen");
                            Chat(command.Channel.Name, ColorGeneral + "dIRCBot Wiki:" + ColorLink + " http://wiki.citizensnpcs.co/dIRCBot");
                        }
                    }
                    break;
                case "log":
                case "logs":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".has_log_page", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Full logs of this channel are available at" + ColorLink + " http://mcmonkey.org/denizen/logs/" + command.Channel.Name.Replace("#", ""));
                    }
                    break;
                case "voice":
                    if (command.User.EverHadVoice && command.Channel.GetUser(Name).OP)
                    {
                        SendCommand("MODE", command.Channel.Name + " " + (command.User.Voice ? "-" : "+") + "v " + command.User.Name);
                    }
                    else
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I'm sorry, " + ColorHighlightMajor + command.User.Name + ColorGeneral + ", I can't do that.");
                    }
                    break;
                case "yml":
                case "yaml":
                    {
                        YAMLCommand(command);
                    }
                    break;
                case "dscript":
                case "ds":
                case "script":
                case "ch":
                case "check":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        dScriptCommand(command);
                    }
                    break;
                case "olddscript":
                case "oldds":
                case "oldscript":
                case "oldch":
                case "oldcheck":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        OLDdScriptCommand(command);
                    }
                    break;
                case "search":
                case "find":
                case "get":
                case "locate":
                case "what":
                case "where":
                case "how":
                case "s":
                case "f":
                case "g":
                case "w":
                case "meta":
                case "metainfo":
                case "docs":
                case "doc":
                case "documentation":
                case "document":
                case "documents":
                case "documentations":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        // TODO
                    }
                    break;
                case "sounds":
                case "sound":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Here is the list of all valid bukkit sounds -" + ColorLink + " http://bit.ly/1AfW8wu");
                    }
                    break;
                case "gs":
                case "getstarted":
                case "start":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, ColorGeneral + "If you're trying to use Denizen for the first time, these web resources will help you (in addition to help received here on this IRC)");
                        Chat(command.Channel.Name, ColorGeneral + "Tutorial Videos[Updated] -" + ColorLink + " " + "http://mcmonkey.org/denizen/vids");
                        Chat(command.Channel.Name, ColorGeneral + "Script Repo[Varies] -" + ColorLink + " " + "http://mcmonkey.org/denizen/repo/list");
                        Chat(command.Channel.Name, ColorGeneral + "Old Script Repo[Outdated] -" + ColorLink + " http://bit.ly/19lCpfV");
                        Chat(command.Channel.Name, ColorGeneral + "Denizen Wiki[Semi-recent] -" + ColorLink + " http://bit.ly/14o3kdq");
                        Chat(command.Channel.Name, ColorGeneral + "Denizen Handbook[Outdated] -" + ColorLink + " http://bit.ly/XaWBLN");
                        Chat(command.Channel.Name, ColorGeneral + "Beginner's Guide[Outdated] -" + ColorLink + " http://bit.ly/1bHkByR");
                    }
                    break;
                case "yes":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Yes.   Si.   Ja.   Oui.   Da.");
                    }
                    break;
                case "mcve":
                case "minimal":
                case "min":
                case "complete":
                case "comp":
                    {
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Please create a Minimal, Complete, and Verifiable Example:" + ColorLink + " https://stackoverflow.com/help/mcve");
                    }
                    break;
                case "myip":
                case "pingip":
                    {
                        MyIPCommand(command);
                    }
                    break;
                case "ping":
                case "mcping":
                    {
                        MCPingCommand(command);
                    }
                    break;
                case "d":
                case "def":
                case "define":
                case "definition":
                    // TODO
                    break;
                case "reload":
                    if (Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".denizen_meta", "false").StartsWith("t"))
                    {
                        LoadMeta(command.Arguments.Count > 0 && command.Arguments[0].ToLower().Contains("monkey"));
                        Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I have reloaded the source files and now have "
                            + ColorHighlightMajor + AllMeta.Commands.Count + ColorGeneral + " commands (" + ExternalMeta.Commands.Count + " external, "
                            + CoreMeta.Commands.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Events.Count + ColorGeneral + " events (" + ExternalMeta.Events.Count + " external, "
                            + CoreMeta.Events.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Tags.Count + ColorGeneral + " tags (" + ExternalMeta.Tags.Count + " external, "
                            + CoreMeta.Tags.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Mechanisms.Count + ColorGeneral + " mechanisms (" + ExternalMeta.Mechanisms.Count + " external, "
                            + CoreMeta.Mechanisms.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Tutorials.Count + ColorGeneral + " tutorials (" + ExternalMeta.Tutorials.Count + " external, "
                            + CoreMeta.Tutorials.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Languages.Count + ColorGeneral + " language infos (" + ExternalMeta.Languages.Count + " external, "
                            + CoreMeta.Languages.Count + " core), "
                            + ColorHighlightMajor + AllMeta.Actions.Count + ColorGeneral + " actions (" + ExternalMeta.Actions.Count + " external, "
                            + CoreMeta.Actions.Count + " core) all loaded!");
                    }
                    break;
                case "quotes":
                case "quote":
                case "qs":
                case "q":
                    // TODO
                    break;
                case "readlogs":
                    if (!command.User.OP
                        || !Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".admin", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, ColorGeneral + "You can't do that!");
                    }
                    else
                    {
                        ReadLogs();
                    }
                    break;
                case "debugtoggle":
                    if (!command.User.OP
                        || !Configuration.Read("dircbot.irc.channels." + command.Channel.Name.Replace("#", "") + ".admin", "false").StartsWith("t"))
                    {
                        Chat(command.Channel.Name, ColorGeneral + "You can't do that!");
                    }
                    else
                    {
                        Logger.Debugging = !Logger.Debugging;
                        Chat(command.Channel.Name, ColorGeneral + "Debugging now " + ColorHighlightMajor + (Logger.Debugging ? "ON" : "OFF"));
                    }
                    break;
                default:
                    // Unknown command.
                    break;
            }
        }
    }
}
