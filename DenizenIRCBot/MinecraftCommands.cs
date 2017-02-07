using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DnDns;
using DnDns.Query;
using DnDns.Records;
using DnDns.Enums;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.IO;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        void MyIPCommand(CommandDetails command)
        {
            if (command.Arguments.Count <= 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <Minecraft server IP address>");
                return;
            }
            command.User.Settings.Set("general.minecraft_server_ip", command.Arguments[0]);
            command.User.Save();
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Recorded your server IP. Use " + ColorHighlightMajor + Prefixes[0] + "mcping <name> " + ColorGeneral + "to ping a player's server!");
        }

        void MCPingCommand(CommandDetails command)
        {
            if (command.Arguments.Count <= 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <Minecraft server IP address>");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            string IP = user.Settings.ReadString("general.minecraft_server_ip", null);
            if (IP == null)
            {
                IP = command.Arguments[0];
            }
            ushort port = 0;
            if (IP.Contains(':'))
            {
                string[] dat = IP.Split(new char[] { ':' }, 2);
                IP = dat[0];
                port = Utilities.StringToUShort(dat[1]);
            }
            if (port == 0)
            {
                try
                {
                    DnsQueryRequest dqr = new DnsQueryRequest();
                    DnsQueryResponse resp = dqr.Resolve("_minecraft._tcp." + IP, NsType.SRV, NsClass.ANY, ProtocolType.Tcp);
                    if (resp != null)
                    {
                        for (int i = 0; i < resp.AdditionalRRecords.Count; i++)
                        {
                            if (resp.AdditionalRRecords[i] is SrvRecord)
                            {
                                port = (ushort)((SrvRecord)resp.AdditionalRRecords[i]).Port;
                            }
                        }
                        for (int i = 0; i < resp.Answers.Length; i++)
                        {
                            if (resp.Answers[i] is SrvRecord)
                            {
                                port = (ushort)((SrvRecord)resp.Answers[i]).Port;
                            }
                        }
                    }
                    else
                    {
                        Logger.Output(LogType.DEBUG, "Null SRV record.");
                    }
                    if (port == 0)
                    {
                        port = (ushort)25565;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Output(LogType.ERROR, "Pinging a SRV record for a minecraft server: " + ex.ToString());
                    port = 25565;
                }
            }
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Stopwatch Timer = new Stopwatch();
                Timer.Reset();
                Timer.Start();
                sock.SendBufferSize = 8192;
                sock.ReceiveBufferSize = 8192 * 10;
                sock.ReceiveTimeout = 3000;
                sock.SendTimeout = 3000;
                IAsyncResult result = sock.BeginConnect(IP, port, null, null);
                bool timeout = !result.AsyncWaitHandle.WaitOne(5000, true);
                if (timeout)
                {
                    throw new Exception("Connection timed out");
                }
                if (!sock.Connected)
                {
                    throw new Exception("Failed to connect");
                }
                byte[] address = UTF8.GetBytes(IP);
                int packlen = 1 + 1 + 1 + address.Length + 2 + 1;
                byte[] send = new byte[1 + packlen + 1 + 1];
                byte[] portbytes = BitConverter.GetBytes(port).Reverse().ToArray();
                send[0] = (byte)packlen; // packet length
                send[1] = (byte)0x00; // Packet ID
                send[2] = (byte)0x04; // Protocol Version
                send[3] = (byte)address.Length; // Address string length
                address.CopyTo(send, 4); // Address string
                portbytes.CopyTo(send, address.Length + 4); // Port
                send[address.Length + 6] = (byte)0x01; // Next state
                send[address.Length + 7] = (byte)0x01; // Next packet length
                send[address.Length + 8] = (byte)0x00; // Empty state request packet ~ packet ID
                sock.Send(send);
                int length = 0;
                // Packet size -> packet ID -> JSON length
                for (int x = 0; x < 2; x++)
                {
                    length = 0;
                    int j = 0;
                    while (true)
                    {
                        byte[] recd = new byte[1];
                        sock.Receive(recd, 1, SocketFlags.None);
                        int k = recd[0];
                        length |= (k & 0x7F) << j++ * 7;
                        if (j > 5) throw new Exception("VarInt too big");
                        if ((k & 0x80) != 128) break;
                        if (Timer.ElapsedMilliseconds > 7000)
                            throw new Exception("Timeout while reading response");
                    }
                    if (x == 0)
                    {
                        byte[] resp = new byte[1];
                        sock.Receive(resp, 1, SocketFlags.None);
                    }
                }
                int gotten = 0;
                byte[] response = new byte[length];
                while (gotten < length)
                {
                    byte[] gotbit = new byte[length - gotten];
                    int newgot = sock.Receive(gotbit, length - gotten, SocketFlags.None);
                    gotbit.CopyTo(response, gotten);
                    gotten += newgot;
                    if (Timer.ElapsedMilliseconds > 7000)
                        throw new Exception("Timeout while reading response");
                }
                string fullresponse = UTF8.GetString(response);
                int ind = fullresponse.IndexOf('{');
                if (ind < 0)
                {
                    throw new Exception("Invalid response packet - is that an actual minecraft server?");
                }
                fullresponse = fullresponse.Substring(ind);
                Dictionary<string, dynamic> dict = new JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(fullresponse.Trim());
                string version = dict["version"]["name"].ToString();
                string versionnum = dict["version"]["protocol"].ToString();
                string players_on = dict["players"]["online"].ToString();
                string players_max = dict["players"]["max"].ToString();
                string description;
                if (dict["description"] is Dictionary<String, Object>)
                {
                    description = dict["description"]["text"].ToString();
                }
                else
                {
                    description = dict["description"].ToString();
                }
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Server: " + IP + ":" + port.ToString() + ", MOTD: '"
                    + Utilities.mctoirc(description) + ColorGeneral + "', players: " + players_on + "/" + players_max + ", version: " + Utilities.mctoirc(version));
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.DEBUG, "Pinging minecraft server: " + ex.ToString());
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Couldn't ping server: internal exception: " + ex.Message);
            }
            finally
            {
                sock.Close();
            }
        }

        private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
        private const string STATUS_API = "https://status.mojang.com/check";

        private const string WEBSITE = "minecraft.net",
                             SESSION = "session.minecraft.net",
                             ACCOUNT = "account.mojang.com",
                             AUTH = "auth.mojang.com",
                             SKINS = "skins.minecraft.net",
                             AUTH_SERVER = "authserver.mojang.com",
                             SESSION_SERVER = "sessionserver.mojang.com",
                             API = "api.mojang.com",
                             TEXTURES = "textures.minecraft.net";

        void MojangStatusCommand(CommandDetails command)
        {
            WebRequest request = WebRequest.Create(STATUS_API);
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    List<Dictionary<string, string>> status = Json.Deserialize<List<Dictionary<string, string>>>(new StreamReader(stream, Encoding.UTF8).ReadToEnd());
                    Dictionary<string, string> allStatus = new Dictionary<string, string>();
                    foreach (Dictionary<string, string> dictionary in status)
                    {
                        foreach (string key in dictionary.Keys)
                        {
                            allStatus.Add(key, dictionary[key]);
                        }
                    }
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral
                        + "Website: " + TranslateStatus(allStatus[WEBSITE]) + "; "
                        + "Session: " + TranslateStatus(allStatus[SESSION]) + "; "
                        + "Account: " + TranslateStatus(allStatus[ACCOUNT]) + "; "
                        + "Auth: " + TranslateStatus(allStatus[AUTH]) + "; "
                        + "Skins: " + TranslateStatus(allStatus[SKINS]) + "; "
                        + "Auth Server: " + TranslateStatus(allStatus[AUTH_SERVER]) + "; "
                        + "Session Server: " + TranslateStatus(allStatus[SESSION_SERVER]) + "; "
                        + "API: " + TranslateStatus(allStatus[API]) + "; "
                        + "Textures: " + TranslateStatus(allStatus[TEXTURES]));
                }
            }
        }

        string TranslateStatus(string status)
        {
            switch (status.ToLower())
            {
                case "green":
                    return S_GREEN + "OK" + ColorGeneral;
                case "yellow":
                    return S_YELLOW + "SLOW" + ColorGeneral;
                case "red":
                    return S_RED + "DOWN" + ColorGeneral;
                default:
                    return "UNKNOWN STATUS";
            }
        }
    }
}
