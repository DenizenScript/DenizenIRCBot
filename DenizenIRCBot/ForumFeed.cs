using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Web;
using System.Net;
using System.Xml;

namespace DenizenIRCBot
{
    public class ForumFeed
    {
        public dIRCBot Bot;

        public ForumFeed(dIRCBot _bot)
        {
            Bot = _bot;
            Thread thread = new Thread(new ThreadStart(MainLoop));
            thread.Start();
        }

        bool announce = false;

        public void MainLoop()
        {
            while (true)
            {
                Thread.Sleep(20000);
                try
                {
                    CheckForumFeed();
                    announce = true;
                }
                catch (Exception ex)
                {
                    Logger.Output(LogType.ERROR, ex.ToString());
                }
            }
        }

        public HashSet<long> pubDatesKnown = new HashSet<long>();

        public void CheckForumFeed()
        {
            string url = dIRCBot.Configuration.ReadString("dircbot.irc-servers." + Bot.ServerName + ".forum.url", null);
            if (url == null || !url.Contains("://"))
            {
                if (!announce)
                {
                    Logger.Output(LogType.INFO, "Not watching forum feed " + Bot.ServerName + " because url is " + url);
                }
                return;
            }
            List<string> channels = dIRCBot.Configuration.ReadStringList("dircbot.irc-servers." + Bot.ServerName + ".forum.channels");
            if (channels == null || channels.Count == 0)
            {
                Logger.Output(LogType.INFO, "Not watching forum feed " + Bot.ServerName + " because no channels");
                return;
            }
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 20000;
            using (WebResponse response = request.GetResponse())
            {
                if (request == null)
                {
                    Logger.Output(LogType.ERROR, "Response null for url " + url);
                    return;
                }
                using (XmlReader reader = XmlReader.Create(response.GetResponseStream()))
                {
                    if (reader == null)
                    {
                        Logger.Output(LogType.ERROR, "Reader null for url " + url);
                        return;
                    }
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    if (feed == null)
                    {
                        Logger.Output(LogType.ERROR, "Feed null for url " + url);
                        return;
                    }
                    foreach (SyndicationItem item in feed.Items)
                    {
                        if (item == null)
                        {
                            Logger.Output(LogType.ERROR, "Item null for url " + url);
                            continue;
                        }
                        long pdate = item.PublishDate.DateTime.ToFileTimeUtc();
                        if (!pubDatesKnown.Contains(pdate))
                        {
                            pubDatesKnown.Add(pdate);
                            string info = Bot.ColorGeneral + "[Forums] " + Bot.ColorHighlightMinor + item.Authors.First().Name
                                + Bot.ColorGeneral + " posted: <<" + Bot.ColorHighlightMajor + item.Title.Text + Bot.ColorGeneral + ">>:"
                                + Bot.ColorLink + " " + (item.Links.Count > 0 ? item.Links.First().Uri.ToString() : "<<no url provided>>");
                            Logger.Output(LogType.INFO, "Found " + info);
                            if (!announce)
                            {
                                Logger.Output(LogType.INFO, "Ignoring...");
                                continue;
                            }
                            foreach (IRCChannel channel in Bot.Channels)
                            {
                                foreach (string chan in channels)
                                {
                                    if (channel.Name.Replace("#", "").ToLowerInvariant() == chan.ToLowerInvariant())
                                    {
                                        Bot.Chat(channel.Name, info, 2);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
