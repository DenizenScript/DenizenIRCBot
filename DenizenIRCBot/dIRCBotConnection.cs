﻿using System;
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
                    try
                    {
                        switch (command.ToLower())
                        {
                            case "ping": // Respond to server-given PING's
                                {
                                    SendCommand("pong", data.Count > 0 ? data[1] : null);
                                }
                                break;
                            case "433": // Nickname In Use
                                {
                                    SendCommand("NICK", Name + "_" + Utilities.random.Next(999));
                                    SendCommand("NS", "identify " + Name + " " + Configuration["dircbot"]["irc"]["nickserv"]["password"]);
                                    SendCommand("NS", "ghost " + Name);
                                    SendCommand("NICK", Name);
                                }
                                break;
                            case "376": // End of MOTD -> Ready To Join And Identify
                                {
                                    SendCommand("NS", "identify " + Configuration["dircbot"]["irc"]["nickserv"]["password"]);
                                    Channels.Clear();
                                    foreach (string channel in BaseChannels)
                                    {
                                        SendCommand("JOIN", "#" + channel);
                                        Logger.Output(LogType.INFO, "Join Channel: #" + channel.ToLower());
                                        Channels.Add(new IRCChannel() { Name = "#" + channel.ToLower() });
                                    }
                                }
                                break;
                            case "477": // Error joining channel
                                {
                                    foreach (string channel in BaseChannels)
                                    {
                                        SendCommand("JOIN", "#" + channel);
                                    }
                                }
                                break;
                            case "332": // Topic for channel
                                {
                                    string channel = data[1].ToLower();
                                    string topic = Utilities.Concat(data, 2).Substring(1);
                                    Logger.Output(LogType.INFO, "Topic for channel: " + channel);
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        if (channel == chan.Name)
                                        {
                                            chan.Topic += topic;
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "topic": // Fresh topic set on channel
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "join": // Someone joined
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "mode": // User mode set
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "353": // User list for channel
                                {
                                    string channel = data[2].ToLower();
                                    List<string> users = new List<string>(data);
                                    users.RemoveRange(0, 4);
                                    Logger.Output(LogType.INFO, "User list for channel: " + channel);
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        if (channel == chan.Name)
                                        {
                                            chan.Users.Clear();
                                            chan.Users.Add(new IRCUser(Name + "!" + Name + "@"));
                                            foreach (string usr in users)
                                            {
                                                Logger.Output(LogType.DEBUG, "Recognize user " + usr);
                                                chan.Users.Add(new IRCUser(usr + "!@"));
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "part": // Someone left the channel
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "quit": // Someone left the server
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "nick": // Someone changed their name
                                {
                                    // TODO: RECORD
                                }
                                break;
                            case "privmsg": // Chat message
                                {
                                    string channel = data[0].ToLower();
                                    string privmsg = Utilities.Concat(data, 1).Substring(1);
                                    Logger.Output(LogType.DEBUG, "User " + user + " spoke in channel " + channel + ", saying " + privmsg);
                                    IRCChannel chan = null;
                                    foreach (IRCChannel chann in Channels)
                                    {
                                        if (chann.Name == channel)
                                        {
                                            chan = chann;
                                        }
                                    }
                                    if (chan == null)
                                    {
                                        return;
                                    }
                                    bool cmd = false;
                                    List<string> cmds = new List<string>(data);
                                    cmds.RemoveAt(0);
                                    string cmdlow = cmds[0].ToLower().Substring(1);
                                    if (cmdlow.StartsWith(Name.ToLower()))
                                    {
                                        Logger.Output(LogType.DEBUG, "Was pinged by " + cmdlow);
                                        cmd = true;
                                        cmds.RemoveAt(0);
                                    }
                                    else
                                    {
                                        foreach (string prefix in Prefixes)
                                        {
                                            if (cmdlow.StartsWith(prefix.ToLower()))
                                            {
                                                cmd = true;
                                                cmds[0] = cmds[0].Substring(1 + prefix.Length);
                                                Logger.Output(LogType.DEBUG, "Recognized " + prefix + " in " + cmds[0]);
                                                break;
                                            }
                                        }
                                    }
                                    if (cmd)
                                    {
                                        CommandDetails details = new CommandDetails();
                                        details.Name = cmds[0];
                                        cmds.RemoveAt(0);
                                        details.Arguments = cmds;
                                        details.Channel = chan;
                                        details.User = chan.GetUser(user);
                                        details.Pinger = "";
                                        if (details.User != null)
                                        {
                                            Logger.Output(LogType.INFO, "Try command " + details.Name + " for " + details.User.Name);
                                            if (cmds.Count > 0)
                                            {
                                                string cmdlast = cmds[cmds.Count - 1];
                                                if (cmdlast.Contains("@"))
                                                {
                                                    string pingme = cmdlast.Replace("@", "");
                                                    IRCUser usr = chan.GetUser(pingme);
                                                    if (usr != null)
                                                    {
                                                        details.Pinger = usr.Name + ": ";
                                                    }
                                                }
                                            }
                                            Task.Factory.StartNew(() =>
                                            {
                                                try
                                                {
                                                    TryCommand(details);
                                                }
                                                catch (Exception ex)
                                                {
                                                    if (ex is ThreadAbortException)
                                                    {
                                                        throw ex;
                                                    }
                                                    Logger.Output(LogType.ERROR, "Command parsing of " + details.Name + ":: " + ex.ToString());
                                                }
                                            });
                                        }
                                        else
                                        {
                                            Logger.Output(LogType.INFO, "Null user sent message to channel!");
                                        }
                                    }
                                }
                                break;
                            case "notice": // NOTICE message
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException)
                        {
                            throw ex;
                        }
                        Logger.Output(LogType.ERROR, "Error: " + ex.ToString());
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