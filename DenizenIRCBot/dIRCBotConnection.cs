using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        /// <summary>
        /// The server address connected to.
        /// </summary>
        public string ServerAddress;

        /// <summary>
        /// The port of the server connected to.
        /// </summary>
        public ushort ServerPort;

        /// <summary>
        /// The socket that connects this bot to IRC.
        /// </summary>
        public Socket IRCSocket;

        /// <summary>
        /// The current name of the bot.
        /// </summary>
        public string Name;

        public List<string> BaseChannels = new List<string>();

        public List<IRCChannel> Channels = new List<IRCChannel>();

        /// <summary>
        /// Connects to the IRC server and runs the bot.
        /// </summary>
        void ConnectAndRun()
        {
            Logger.Output(LogType.INFO, "Connecting to " + ServerAddress + ":" + ServerPort + " as user " + Name + "...");
            IRCSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IRCSocket.Connect(ServerAddress, ServerPort);
            Logger.Output(LogType.INFO, "Connected to " + IRCSocket.RemoteEndPoint.ToString());
            string host = Configuration["dircbot"]["irc"]["host"];
            SendCommand("USER", Name + " " + host + " " + host + " :" + Name);
            SendCommand("NICK", Name);
            string receivedAlready = string.Empty;
            while (true)
            {
                while (IRCSocket.Available <= 0)
                {
                    Thread.Sleep(1);
                }
                int avail = IRCSocket.Available;
                byte[] receivedNow = new byte[avail > 1024 ? 1024: avail];
                IRCSocket.Receive(receivedNow, receivedNow.Length, SocketFlags.None);
                string got = UTF8.GetString(receivedNow).Replace("\r", "");
                Logger.Output(LogType.DEBUG, "Recieved " + avail + ", which yielded " + got.Replace("\n", "\\n"));
                receivedAlready += got;
                while (receivedAlready.Contains('\n'))
                {
                    int index = receivedAlready.IndexOf('\n');
                    string message = receivedAlready.Substring(0, index);
                    receivedAlready = receivedAlready.Substring(index + 1);
                    Logger.Output(LogType.DEBUG, "Received message: " + message);
                    List<string> data = message.Split(' ').ToList();
                    string user = "";
                    string command = data[0];
                    if (command.StartsWith(":"))
                    {
                        user = command.Substring(1);
                        data.RemoveAt(0);
                        if (data.Count > 0)
                        {
                            command = data[0];
                            data.RemoveAt(0);
                        }
                        else
                        {
                            command = "null";
                        }
                    }
                    switch (command.ToLower())
                    {
                        case "ping": // Respond to server-given PING's
                            SendCommand("pong", data.Count > 0 ? data[1]: null);
                            break;
                        case "433": // Nickname In Use
                            SendCommand("NICK", Name + "_" + Utilities.random.Next(999));
                            SendCommand("NS", "identify " + Name + " " + Configuration["dircbot"]["irc"]["nickserv"]["password"]);
                            SendCommand("NS", "ghost " + Name);
                            SendCommand("NICK", Name);
                            break;
                        case "376": // Ready To Join And Identify
                            SendCommand("NS", "identify " + Configuration["dircbot"]["irc"]["nickserv"]["password"]);
                            foreach (string channel in BaseChannels)
                            {
                                SendCommand("JOIN", "#" + channel);
                            }
                            break;
                        case "477": // Error joining channel
                            foreach (string channel in BaseChannels)
                            {
                                SendCommand("JOIN", "#" + channel);
                            }
                            break;
                        case "332": // Topic for channel
                            // TODO: RECORD
                            break;
                        case "topic": // Fresh topic set on channel
                            // TODO: RECORD
                            break;
                        case "join": // Someone joined
                            // TODO: RECORD
                            break;
                        case "mode": // User mode set
                            // TODO: RECORD
                            break;
                        case "353": // User list for channel
                            // TODO: RECORD
                            break;
                        case "part": // Someone left the channel
                            break;
                        case "quit": // Someone left the server
                            break;
                        case "nick": // Someone changed their name
                            break;
                        case "privmsg": // Chat message
                            {
                                string privmsg = Utilities.Concat(data, 1).Substring(1);
                                Logger.Output(LogType.DEBUG, "User " + user + " spoke in channel " + data[0] + ", saying " + privmsg);
                            }
                            break;
                        case "notice": // NOTICE message
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Lock all socket operations around this object.
        /// </summary>
        Object SocketLocker = new Object();

        /// <summary>
        /// Send a command to te server.
        /// </summary>
        public void SendCommand(string command, string data)
        {
            lock (SocketLocker)
            {
                if (string.IsNullOrEmpty(data))
                {
                    Logger.Output(LogType.DEBUG, "Sent: " + Encoding.ASCII.GetString(UTF8.GetBytes(command.ToUpper())));
                    IRCSocket.Send(UTF8.GetBytes(command.ToUpper() + "\n"));
                }
                else
                {
                    Logger.Output(LogType.DEBUG, "Sent: " + Encoding.ASCII.GetString(UTF8.GetBytes(command.ToUpper() + " " + data)));
                    IRCSocket.Send(UTF8.GetBytes(command.ToUpper() + " " + data + "\n"));
                }
            }
        }

        /// <summary>
        /// Send a chat message to a channel.
        /// Optionally, specify a maximum message split up.
        /// </summary>
        public int Chat(string channel, string message, int limit = 10)
        {
            if (limit <= 0)
            {
                return 0;
            }
            if (message.Length > 400)
            {
                string first = message.Substring(0, 400);
                string second = ColorGeneral + "[Continued] " + message.Substring(400);
                int chats = Chat(channel, first, limit);
                chats = Chat(channel, second, chats);
                return chats;
            }
            SendCommand("PRIVMSG", channel + " :" + message);
            return limit - 1;
        }

        /// <summary>
        /// Send an IRC NOTICE to a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        public void Notice(string user, string data)
        {
            SendCommand("NOTICE", user + " :" + data);
        }
    }
}
