using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using System.Text.RegularExpressions;

namespace DenizenIRCBot
{
    /// <summary>
    /// The core systems of the IRC Bot.
    /// </summary>
    public partial class dIRCBot
    {
        public static Encoding UTF8 = new UTF8Encoding(false);

        public static dDiscordBot DiscordBot;

        public static List<dIRCBot> PresentBots = new List<dIRCBot>();

        public static Object presentBotsLock = new Object();

        public static void DiscordMessage(ulong channel, string author, string message)
        {
            lock (presentBotsLock)
            {
                foreach (dIRCBot bot in PresentBots)
                {
                    ulong rch = ulong.Parse(Configuration.ReadString("dircbot.irc-servers." + bot.ServerName + ".discord_bridge.discord_channel", "0"));
                    if (rch != 0 && channel == rch)
                    {
                        string ich = Configuration.ReadString("dircbot.irc-servers." + bot.ServerName + ".discord_bridge.irc_channel", null);
                        if (ich != null)
                        {
                            bot.Chat(ich, bot.ColorGeneral + "[Discord] <" + bot.ColorHighlightMajor + author + bot.ColorGeneral + "> " + bot.ColorHighlightMinor + message, 2);
                        }
                    }
                }
            }
        }

        static Regex stripColor = new Regex(C_S_COLOR + "[0-9][0-9]?", RegexOptions.Compiled);

        public void OnMessage(string channel, string author, string message)
        {
            string ich = Configuration.ReadString("dircbot.irc-servers." + ServerName + ".discord_bridge.irc_channel", null);
            if (ich != null && ich.ToLowerInvariant() == channel.ToLowerInvariant())
            {
                ulong rch = ulong.Parse(Configuration.ReadString("dircbot.irc-servers." + ServerName + ".discord_bridge.discord_channel", "0"));
                if (rch != 0)
                {
                    DiscordBot.Message(rch, "[IRC] <" + author + "> " + stripColor.Replace(message, ""));
                }
            }
        }

        /// <summary>
        /// Global program entry point.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing!");
            Configuration = new YAMLConfiguration(GetConfig());
            List<Task> tasks = new List<Task>();
            DiscordBot = new dDiscordBot();
            Task.Factory.StartNew(() => DiscordBot.Init(Configuration));
            foreach (string server in Configuration.GetKeys("dircbot.irc-servers"))
            {
                Console.WriteLine("Preparing server: " + server);
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    dIRCBot core = new dIRCBot();
                    lock (presentBotsLock)
                    {
                        PresentBots.Add(core);
                    }
                    core.ServerName = server;
                    core.Init();
                }));
            }
            while (tasks.Count > 0)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (tasks[i].IsCompleted)
                    {
                        tasks.RemoveAt(i--);
                    }
                }
                Thread.Sleep(100);
            }
            Logger.Output(LogType.INFO, "Quitting cleanly.");
        }

        public string ServerName;

        /// <summary>
        /// Reads the config file as a string.
        /// </summary>
        static string GetConfig()
        {
            try
            {
                return File.ReadAllText("config.yml");
            }
            catch (Exception)
            {
                Logger.Output(LogType.ERROR, "Failed to read config.yml!");
                return "";
            }
        }

        /// <summary>
        /// The bot's configuration data (config.yml).
        /// </summary>
        public static YAMLConfiguration Configuration;

        public string[] Prefixes = null;

        void PrepareConfig()
        {
            try
            {
                ServerAddress = Configuration.ReadString("dircbot.irc-servers." + ServerName + ".server", "");
                ServerPort = Utilities.StringToUShort(Configuration.ReadString("dircbot.irc-servers." + ServerName + ".port", ""));
                Name = Configuration.ReadString("dircbot.irc-servers." + ServerName + ".username", "");
                BaseChannels.Clear();
                foreach (string channel in Configuration.GetKeys("dircbot.irc-servers." + ServerName + ".channels"))
                {
                    BaseChannels.Add(channel);
                }
                Prefixes = Configuration.ReadStringList("dircbot.prefixes").ToArray();
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Failed to load config: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        public ForumFeed Forum;
        
        /// <summary>
        /// Launches the IRC Bot.
        /// This will freeze the current thread until the bot is shut down.
        /// </summary>
        public void Init()
        {
            try
            {
                PrepareConfig();
                LoadMeta(false);
                InitGitHub();
                Forum = new ForumFeed(this);
                Bitly.Init(Configuration.ReadString("dircbot.bitly.main", ""), Configuration.ReadString("dircbot.bitly.backup", ""));
                WolframAlpha.Init(Configuration.ReadString("dircbot.wolfram.appid", ""));
                if (string.IsNullOrEmpty(ServerAddress))
                {
                    Logger.Output(LogType.ERROR, "No address given, quitting.");
                    return;
                }
                while (true)
                {
                    try
                    {
                        ConnectAndRun();
                        throw new Exception("Somehow escaped while loop?");
                    }
                    catch (Exception ex)
                    {
                        if (ex is ThreadAbortException)
                        {
                            throw ex;
                        }
                        Logger.Output(LogType.ERROR, "Error in primary run: " + ex.ToString());
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Full exception: " + ex.ToString());
            }
            finally
            {
                Logger.Output(LogType.ERROR, "RIP IN PEACE, BOT NOW DEAD!");
            }
        }
    }
}
