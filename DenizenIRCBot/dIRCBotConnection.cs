using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

        public Dictionary<string, Dictionary<string, List<string>>> RecentMessages = new Dictionary<string, Dictionary<string, List<string>>>();

        volatile bool resending = false;

        /// <summary>
        /// Connects to the IRC server and runs the bot.
        /// </summary>
        void ConnectAndRun()
        {
            Logger.Output(LogType.INFO, "Connecting to " + ServerAddress + ":" + ServerPort + " as user " + Name + "...");
            IRCSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IRCSocket.Connect(ServerAddress, ServerPort);
            Logger.Output(LogType.INFO, "Connected to " + IRCSocket.RemoteEndPoint.ToString());
            string host = Configuration.ReadString("dircbot.irc-servers." + ServerName + ".host", "unknown");
            SendCommand("USER", Name + " " + host + " " + host + " :" + Name);
            SendCommand("NICK", Name);
            string receivedAlready = string.Empty;
            while (true)
            {
                long timePassed = 0;
                bool pinged = false;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (IRCSocket.Available <= 0)
                {
                    sw.Stop();
                    timePassed += sw.ElapsedMilliseconds;
                    if (timePassed > 60 * 1000 && !pinged)
                    {
                        SendCommand("PING", Utilities.random.Next(10000).ToString());
                        pinged = true;
                    }
                    if (timePassed > 120 * 1000)
                    {
                        throw new Exception("Ping timed out!");
                    }
                    sw.Reset();
                    sw.Start();
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
                                    SendCommand("PONG", data.Count > 0 ? data[1] : null);
                                }
                                break;
                            case "433": // Nickname In Use
                                {
                                    SendCommand("NICK", Name + "_" + Utilities.random.Next(999));
                                    SendCommand("NS", "identify " + Name + " " + Configuration.Read("dircbot.irc-servers." + ServerName + ".nickserv.password", ""));
                                    SendCommand("NS", "ghost " + Name);
                                    SendCommand("NICK", Name);
                                }
                                break;
                            case "376": // End of MOTD -> Ready To Join And Identify
                                {
                                    SendCommand("NS", "identify " + Configuration.Read("dircbot.irc-servers." + ServerName + ".nickserv.password", ""));
                                    Channels.Clear();
                                    foreach (string channel in BaseChannels)
                                    {
                                        SendCommand("JOIN", "#" + channel);
                                        Logger.Output(LogType.INFO, "Join Channel: #" + channel.ToLower());
                                        IRCChannel chan = new IRCChannel() { Name = "#" + channel.ToLower() };
                                        Channels.Add(chan);
                                    }
                                }
                                break;
                            case "477": // Error joining channel
                                resending = true;
                                Task.Factory.StartNew(() =>
                                {
                                    Thread.Sleep(5000);
                                    foreach (string channel in BaseChannels)
                                    {
                                        SendCommand("JOIN", "#" + channel);
                                    }
                                    resending = false;
                                });
                                break;
                            case "332": // Topic for channel
                                {
                                    if (!resending)
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            Thread.Sleep(5000);
                                            foreach (string achannel in BaseChannels)
                                            {
                                                SendCommand("JOIN", "#" + achannel);
                                            }
                                            resending = false;
                                        });
                                    }
                                    resending = true;
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
                                    string channel = data[0].ToLower();
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        if (channel == chan.Name)
                                        {
                                            IRCUser newuser = new IRCUser(user);
                                            SeenUser(newuser.Name, newuser.IP);
                                            chan.Users.Add(newuser);
                                            Logger.Output(LogType.DEBUG, "Recognizing join of " + newuser.Name + " into " + chan.Name);
                                            if (Configuration.ReadString("dircbot.irc-servers." + ServerName + ".channels." + chan.Name.Replace("#", "") + ".greet", "false").StartsWith("t"))
                                            {
                                                foreach (string msg in Configuration.ReadStringList("dircbot.irc-servers." + ServerName + ".channels." + chan.Name.Replace("#", "") + ".greeting"))
                                                {
                                                    Notice(newuser.Name, msg.Replace("<BASE>", ColorGeneral).Replace("<MAJOR>", ColorHighlightMajor).Replace("<MINOR>", ColorHighlightMinor));
                                                }
                                            }
                                            break;
                                        }
                                    }
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
                                            chan.Users.Add(new IRCUser(Name + "!" + Name + "@"));
                                            foreach (string usr in users)
                                            {
                                                Logger.Output(LogType.DEBUG, "Recognize user " + usr);
                                                chan.Users.Add(new IRCUser(usr));
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            case "part": // Someone left the channel
                                {
                                    string channel = data[0].ToLower();
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        if (channel == chan.Name)
                                        {
                                            IRCUser quitter = new IRCUser(user);
                                            string name = quitter.Name.ToLower();
                                            for (int i = 0; i < chan.Users.Count; i++)
                                            {
                                                if (chan.Users[i].Name.ToLower() == name)
                                                {
                                                    Logger.Output(LogType.DEBUG, "Recognizing leave of " + chan.Users[i].Name + " from " + chan.Name);
                                                    chan.Users.RemoveAt(i--);
                                                    break;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                                // TODO: Kick
                            case "quit": // Someone left the server
                                {
                                    IRCUser quitter = new IRCUser(user);
                                    string name = quitter.Name.ToLower();
                                    // quitreason = concat(data, 0).substring(1);
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        for (int i = 0; i < chan.Users.Count; i++)
                                        {
                                            if (chan.Users[i].Name.ToLower() == name)
                                            {
                                                Logger.Output(LogType.DEBUG, "Recognizing quit of " + chan.Users[i].Name + " from " + chan.Name);
                                                chan.Users.RemoveAt(i--);
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "nick": // Someone changed their name
                                {
                                    IRCUser renamer = new IRCUser(user);
                                    string nicknew = data[0].Substring(1);
                                    string name = renamer.Name.ToLower();
                                    renamer.SetSeen("renaming to " + nicknew);
                                    foreach (IRCChannel chan in Channels)
                                    {
                                        for (int i = 0; i < chan.Users.Count; i++)
                                        {
                                            if (chan.Users[i].Name.ToLower() == name)
                                            {
                                                Logger.Output(LogType.DEBUG, "Recognizing rename of " + chan.Users[i].Name + " to " + nicknew);
                                                SeenUser(nicknew, renamer.IP);
                                                IRCUser theusr = chan.Users[i];
                                                chan.Users.RemoveAt(i--);
                                                chan.Users.Add(new IRCUser(nicknew + "!" + theusr.Ident + "@" + theusr.IP) { Voice = theusr.Voice, OP = theusr.OP, EverHadVoice = theusr.EverHadVoice });
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "privmsg": // Chat message
                                {
                                    string channel = data[0].ToLower();
                                    data[1] = data[1].Substring(1);
                                    string privmsg = Utilities.Concat(data, 1);
                                    Logger.Output(LogType.INFO, "User " + user + " spoke in channel " + channel + ", saying " + privmsg);
                                    if (privmsg == actionchr + "VERSION" + actionchr)
                                    {
                                        Notice(user.Substring(0, user.IndexOf('!')), actionchr.ToString() + "VERSION " + Configuration.Read("dircbot.version", "DenizenBot vMisconfigured") + actionchr.ToString());
                                    }
                                    else if (privmsg.StartsWith(actionchr + "PING "))
                                    {
                                        Notice(user.Substring(0, user.IndexOf('!')), privmsg);
                                    }
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
                                        break;
                                    }
                                    IRCUser iuser = chan.GetUser(user);
                                    Match match = Regex.Match(privmsg, "^s/([^/]+)/([^/]+)/?([^\\s/]+)?", RegexOptions.IgnoreCase);
                                    if (match.Success)
                                    {
                                        string value = match.Groups[1].Value;
                                        string s_user = match.Groups[3].Success ? match.Groups[3].Value.ToLower() : null;
                                        if (RecentMessages.ContainsKey(channel))
                                        {
                                            int num = -1;
                                            string final = null;
                                            foreach (KeyValuePair<string, List<string>> kvp in RecentMessages[channel])
                                            {
                                                if (s_user != null && kvp.Key.ToLower() != s_user)
                                                {
                                                    continue;
                                                }
                                                for (int i = 0; i < kvp.Value.Count; i++)
                                                {
                                                    if (Regex.Match(kvp.Value[i], value, RegexOptions.IgnoreCase).Success)
                                                    {
                                                        num = i;
                                                        final = Regex.Replace(kvp.Value[i], value, match.Groups[2].Value, RegexOptions.IgnoreCase);
                                                        s_user = kvp.Key;
                                                        goto end_s;
                                                    }
                                                }
                                            }
                                            end_s:
                                            if (num != -1)
                                            {
                                                RecentMessages[channel][s_user][num] = final;
                                                Chat(chan.Name, ColorGeneral + "<" + s_user + "> " + final);
                                                goto post_s;
                                            }
                                        }
                                    }
                                    if (!RecentMessages.ContainsKey(channel))
                                    {
                                        RecentMessages[channel] = new Dictionary<string, List<string>>();
                                    }
                                    if (!RecentMessages[channel].ContainsKey(iuser.Name))
                                    {
                                        RecentMessages[channel][iuser.Name] = new List<string>();
                                    }
                                    RecentMessages[channel][iuser.Name].Add(privmsg);
                                    post_s:
                                    if (Configuration.ReadString("dircbot.irc-servers." + ServerName + ".channels." + chan.Name.Replace("#", "") + ".link_read", "false").StartsWith("t"))
                                    {
                                        foreach (string str in data)
                                        {
                                            if (str.StartsWith("http://") || str.StartsWith("https://"))
                                            {
                                                Task.Factory.StartNew(() =>
                                                {
                                                    try
                                                    {
                                                        LowTimeoutWebclient ltwc = new LowTimeoutWebclient();
                                                        ltwc.Encoding = UTF8;
                                                        string web = ltwc.DownloadString(str);
                                                        if (web.Contains("<title>") && web.Contains("</title>"))
                                                        {
                                                            web = web.Substring(web.IndexOf("<title>") + 7);
                                                            web = web.Substring(0, web.IndexOf("</title>"));
                                                            web = web.Replace("\r", "").Replace("\n", "");
                                                            web = web.Replace("&lt;", "<").Replace("&gt;", ">");
                                                            web = web.Replace("&quot;", "\"").Replace("&amp;", (char)0x01 + "amp");
                                                            string webtitle = "";
                                                            bool flip = false;
                                                            for (int x = 0; x < web.Length; x++)
                                                            {
                                                                if (web[x] == '&')
                                                                {
                                                                    flip = true;
                                                                    continue;
                                                                }
                                                                else if (web[x] == ';')
                                                                {
                                                                    if (flip)
                                                                    {
                                                                        flip = false;
                                                                        continue;
                                                                    }
                                                                }
                                                                else if (web[x] == ' ' && x > 0 && web[x - 1] == ' ')
                                                                {
                                                                    continue;
                                                                }
                                                                if (!flip)
                                                                {
                                                                    webtitle += web[x].ToString();
                                                                }
                                                            }
                                                            webtitle = webtitle.Trim().Replace("Citizens", "Cit.izens").Replace((char)0x01 + "amp", "&");
                                                            Chat(chan.Name, ColorGeneral + "Title --> " + ColorHighlightMinor + webtitle.Trim(), 1);
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Logger.Output(LogType.DEBUG, "Failed to read webpage " + str + ": " + ex.ToString());
                                                    }
                                                });
                                            }
                                        }
                                    }
                                    if (iuser == null)
                                    {
                                        Logger.Output(LogType.INFO, "Null user sent message to channel!");
                                        break;
                                    }
                                    if (string.IsNullOrEmpty(iuser.IP))
                                    {
                                        iuser.ParseMask(user);
                                    }
                                    CheckReminders(iuser, chan);
                                    if (Configuration.ReadString("dircbot.irc-servers." + ServerName + ".channels." + chan.Name.Replace("#", "") + ".record_seen", "false").StartsWith("t"))
                                    {
                                        Task.Factory.StartNew(() =>
                                        {
                                            try
                                            {
                                                iuser.SetSeen("in " + chan.Name + ", saying " + privmsg);
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Output(LogType.ERROR, "SEEN user " + iuser.OriginalMask + ": " + ex.ToString());
                                            }
                                        });
                                    }
                                    if (Configuration.ReadString("dircbot.irc-servers." + ServerName + ".channels." + chan.Name.Replace("#", "") + ".has_log_page", "false").StartsWith("t"))
                                    {
                                        Log(chan.Name, Utilities.FormatDate(DateTime.Now) + " <" + iuser.Name + "> " + privmsg);
                                    }
                                    bool cmd = false;
                                    List<string> cmds = new List<string>(data);
                                    cmds.RemoveAt(0);
                                    string cmdlow = cmds[0].ToLower();
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
                                                cmds[0] = cmds[0].Substring(prefix.Length);
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
                                        details.User = iuser;
                                        details.Pinger = "";
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
                                                    cmds.RemoveAt(cmds.Count - 1);
                                                }
                                            }
                                        }
                                        new Task(() =>
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
                                        }).Start();
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
                    Logger.Output(LogType.DEBUG, "Sent: " + command.ToUpper());
                    IRCSocket.Send(UTF8.GetBytes(command.ToUpper() + "\n"));
                }
                else
                {
                    if (command.ToUpper() != "NS")
                    {
                        Logger.Output(LogType.DEBUG, "Sent: " + command.ToUpper() + " " + data);
                    }
                    else
                    {
                        Logger.Output(LogType.DEBUG, "Sent NS command");
                    }
                    IRCSocket.Send(UTF8.GetBytes(command.ToUpper() + " " + data + "\n"));
                }
            }
        }

        Object LogLock = new Object();

        public void Log(string channel, string message)
        {
            lock (LogLock)
            {
                DateTime now = DateTime.Now;
                string fold = "logs/" + ServerName.ToLower() + "/" + channel.ToLower().Replace("#", "") + "/" + now.Year + "/" + now.Month + "/";
                System.IO.Directory.CreateDirectory(fold);
                System.IO.File.AppendAllText(fold + now.Day + ".log", message + "\n");
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
                // TODO: possibly split on the word, not the letter?
                string first = message.Substring(0, 400);
                string second = ColorGeneral + "[Continued] " + message.Substring(400);
                int chats = Chat(channel, first, limit);
                chats = Chat(channel, second, chats);
                return chats;
            }
            foreach (string ping in Configuration.ReadStringList("dircbot.irc-servers." + ServerName + ".channels." + channel.Replace("#", "") + ".anti_ping", new List<string>()))
            {
                int num = -2;
                while (true)
                {
                    num = message.IndexOf(ping, num + 2);
                    if (num != -1)
                    {
                        message = message.Insert(num + 1, ".");
                        continue;
                    }
                    break;
                }
            }
            SendCommand("PRIVMSG", channel + " :" + message.Replace("\n", "\\n").Replace("\r", "\\r"));
            if (Configuration.ReadString("dircbot.irc-servers." + ServerName + ".channels." + channel.Replace("#", "") + ".has_log_page", "false").StartsWith("t"))
            {
                Log(channel, Utilities.FormatDate(DateTime.Now) + " <" + Name + "> " + message.Replace("\n", "\\n").Replace("\r", "\\r"));
            }
            return limit - 1;
        }

        /// <summary>
        /// Send an IRC NOTICE to a user.
        /// </summary>
        public void Notice(string user, string data)
        {
            SendCommand("NOTICE", user + " :" + data);
        }
    }
}
