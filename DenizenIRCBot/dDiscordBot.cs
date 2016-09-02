using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DenizenIRCBot
{
    public class dDiscordBot
    {
        public YAMLConfiguration Config;

        DiscordClient Client;

        public void Init(YAMLConfiguration conf)
        {
            try
            {
                Config = conf;
                string token = Config.ReadString("discord.token", null);
                if (token == null)
                {
                    Logger.Output(LogType.INFO, "Discord bot not configured!");
                    return;
                }
                Client = new DiscordClient();
                Client.MessageReceived += messageReceived;
                Client.ExecuteAndWait(async () => await Client.Connect(token, TokenType.Bot));
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, ex.ToString());
            }
        }

        public void Message(ulong channel, string message)
        {
            if (Client == null)
            {
                return;
            }
            Channel c = Client.GetChannel(channel);
            if (c == null)
            {
                return;
            }
            c.SendMessage(message).Wait();
        }

        public void messageReceived(object sender, MessageEventArgs e)
        {
            // Don't handle to our own messages, ever.
            if (e.Message.IsAuthor)
            {
                return;
            }
            Logger.Output(LogType.INFO, "Discord message: " + e.User.Name + ": " + e.Message.RawText);
            dIRCBot.DiscordMessage(e.Channel.Id, e.User.Name, e.Message.RawText);
        }
    }
}
