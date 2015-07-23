using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public void MessageCommand(CommandDetails command)
        {
            if (command.Arguments.Count <= 1)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name> <message>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name + " mcmonkey Hi, how are you?");
                return;
            }
            if (!IRCUser.Exists(command.Arguments[0]))
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I have never seen that user.");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            user.SendReminder(command.User.Name + ": " + Utilities.Concat(command.Arguments, 1), DateTime.Now);
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Message stored.");
        }

        public void ReminderCommand(CommandDetails command)
        {
            if (command.Arguments.Count <= 2)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor + Prefixes[0] + command.Name + " <name> <time> <message>");
                Chat(command.Channel.Name, ColorGeneral + "For example, " + ColorHighlightMajor + Prefixes[0] + command.Name + " mcmonkey 1d Download the code update");
                return;
            }
            if (!IRCUser.Exists(command.Arguments[0]))
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I have never seen that user.");
                return;
            }
            IRCUser user = new IRCUser(command.Arguments[0]);
            TimeSpan rel = Utilities.StringToDuration(command.Arguments[1]);
            user.SendReminder(command.User.Name + ": " + Utilities.Concat(command.Arguments, 2), DateTime.Now.Add(rel));
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Reminder stored.");
        }

        public void CheckReminders(IRCUser user, IRCChannel channel)
        {
            List<string> rem = user.GetReminders();
            if (rem.Count > 30)
            {
                Chat(channel.Name, user.Name + ": You are about to receive " + rem.Count + " messages. Please tell an admin if you are being spammed!");
            }
            if (rem.Count > 0)
            {
                Chat(channel.Name, user.Name + ": I have messages for you!");
                Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < rem.Count; i++)
                    {
                        Notice(user.Name, rem[i]);
                        Thread.Sleep(500);
                    }
                });
            }
        }
    }
}
