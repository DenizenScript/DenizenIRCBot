using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace DenizenIRCBot
{
    /// <summary>
    /// The core systems of the IRC Bot.
    /// </summary>
    public partial class dIRCBot
    {
        public static Encoding UTF8 = new UTF8Encoding(false);

        /// <summary>
        /// Global program entry point.
        /// </summary>
        static void Main(string[] args)
        {
            dIRCBot core = new dIRCBot();
            core.Init();
        }

        /// <summary>
        /// Reads the config file as a string.
        /// </summary>
        string GetConfig()
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
        public YAMLConfiguration Configuration;

        public string[] Prefixes = null;

        void PrepareConfig()
        {
            try
            {
                Deserializer des = new Deserializer();
                Configuration = new YAMLConfiguration(GetConfig());
                ServerAddress = Configuration.ReadString("dircbot.irc.server", "");
                ServerPort = Utilities.StringToUShort(Configuration.ReadString("dircbot.irc.port", ""));
                Name = Configuration.ReadString("dircbot.irc.username", "");
                BaseChannels.Clear();
                foreach (string channel in Configuration.GetKeys("dircbot.irc.channels"))
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
        
        /// <summary>
        /// Launches the IRC Bot.
        /// This will freeze the current thread until the bot is shut down.
        /// </summary>
        public void Init()
        {
            PrepareConfig();
            LoadMeta(false);
            InitGitHub();
            Bitly.Init(Configuration.ReadString("dircbot.bitly.main", ""), Configuration.ReadString("dircbot.bitly.backup", ""));
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
    }
}
