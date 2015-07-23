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
        public Dictionary<string, dynamic> Configuration;

        void PrepareConfig()
        {
            try
            {
                Deserializer des = new Deserializer();
                Configuration = des.Deserialize<Dictionary<string, dynamic>>(new StringReader(GetConfig()));
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
            while (true)
            {
                try
                {
                    ServerAddress = Configuration["dircbot"]["irc"]["server"];
                    ServerPort = Utilities.StringToUShort(Configuration["dircbot"]["irc"]["port"]);
                    Name = Configuration["dircbot"]["irc"]["username"];
                    foreach (string channel in Configuration["dircbot"]["irc"]["channels"].Keys)
                    {
                        BaseChannels.Add(Configuration["dircbot"]["irc"]["channels"][channel]["name"]);
                    }
                    ConnectAndRun();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        throw ex;
                    }
                    Logger.Output(LogType.ERROR, "Error in primary run: " + ex.ToString());
                }
            }
        }
    }
}
