using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        void SeenCommand(CommandDetails command)
        {
            if (command.Arguments.Count == 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name +" mcmonkey");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            string seen = user.GetSeen(1);
            if (seen == null)
            {
                Logger.Output(LogType.DEBUG, "Never before seen " + user.Name);
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I've never seen that user!");
                return;
            }
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I last saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
        }

        void RecentCommand(CommandDetails command)
        {
            if (command.Arguments.Count == 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name + " mcmonkey");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            string seen = user.GetSeen(1);
            if (seen == null)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I've never seen that user!");
                return;
            }
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I last saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
            for (int i = 2; i <= 5; i++)
            {
                seen = user.GetSeen(i);
                if (seen != null)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Previously, I saw " + ColorHighlightMajor + user.Name + ColorGeneral + " at " + ColorHighlightMinor + seen, 3);
                }
            }
        }
    }
}
