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
            Console.WriteLine("Initializing!");
            Configuration = new YAMLConfiguration(GetConfig());
            List<Task> tasks = new List<Task>();
            foreach (string server in Configuration.GetKeys("dircbot.irc-servers"))
            {
                Console.WriteLine("Preparing server: " + server);
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    dIRCBot core = new dIRCBot();
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
                Thread.Sleep(1);
            }
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
