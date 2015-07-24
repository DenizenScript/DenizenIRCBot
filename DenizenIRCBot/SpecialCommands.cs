using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public void ReadLogs()
        {
            string[] logsets = Directory.GetFileSystemEntries(Environment.CurrentDirectory + "/data/logs");
            foreach (string channel in logsets)
            {
                Logger.Output(LogType.DEBUG, "Found channel: " + channel);
                string[] years = Directory.GetFileSystemEntries(channel);
                foreach (string str_year in years)
                {
                    Logger.Output(LogType.DEBUG, "Found year: " + str_year);
                    string[] months = Directory.GetFileSystemEntries(str_year);
                    foreach (string str_month in months)
                    {
                        Logger.Output(LogType.DEBUG, "Found month: " + str_month);
                        string[] days = Directory.GetFileSystemEntries(str_month);
                        foreach (string str_day in days)
                        {
                            try
                            {
                                Logger.Output(LogType.DEBUG, "Found day: " + str_day);
                                string file = File.ReadAllText(str_day);
                                string[] file_split = file.Split('\n');
                                bool reading = false;
                                for (int i = 0; i < file_split.Length; i++)
                                {
                                    if (!reading && file_split[i].Trim() == "")
                                    {
                                        reading = true;
                                    }
                                    else if (reading)
                                    {
                                        string data = file_split[i];
                                        bool is_old = data.StartsWith(C_S_COLOR + "");
                                        for (int x = 0; x < 16; x++)
                                        {
                                            data = data.Replace(C_S_COLOR + Utilities.FormatNumber(x), "");
                                        }
                                        string[] splits = data.Split(new char[] { ' ' }, 4);
                                        if (splits.Length == 4 && !splits[2].Contains("*"))
                                        {
                                            string[] date = splits[0].Split('/');
                                            string[] time = splits[1].Split(':');
                                            if (time.Length != 3)
                                            {
                                                Logger.Output(LogType.DEBUG, "Found " + splits[1] + " instead of a 3-length timestamp");
                                            }
                                            else
                                            {
                                                string name = splits[2].Substring(1, splits[2].Length - 2);
                                                string message = splits[3];
                                                IRCUser user = new IRCUser(name);
                                                string sy = str_year.Substring(str_year.LastIndexOf('\\') + 1).Replace(".log", "");
                                                sy = sy.Substring(sy.LastIndexOf('/') + 1);
                                                string sm = str_month.Substring(str_month.LastIndexOf('\\') + 1).Replace(".log", "");
                                                sm = sm.Substring(sm.LastIndexOf('/') + 1);
                                                string sd = str_day.Substring(str_day.LastIndexOf('\\') + 1).Replace(".log", "");
                                                sd = sd.Substring(sd.LastIndexOf('/') + 1);
                                                int y = Utilities.StringToInt(sy);
                                                int m = Utilities.StringToInt(sm);
                                                int d = Utilities.StringToInt(sd);
                                                DateTime dt = new DateTime(y, m, d, Utilities.StringToInt(time[0]), Utilities.StringToInt(time[1]), Utilities.StringToInt(time[2]));
                                                string ch = channel.Substring(channel.LastIndexOf('\\') + 1);
                                                ch = ch.Substring(ch.LastIndexOf('/') + 1);
                                                user.SetSeen("in #" + ch + ", saying " + message, dt);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Output(LogType.ERROR, "Failed to read " + str_year + "/" + str_month + "/" + str_day + ": " + ex.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
