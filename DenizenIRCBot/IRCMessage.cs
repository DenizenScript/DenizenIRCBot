using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public class IRCMessage
    {
        public IRCChannel Channel;
        public IRCUser User;
        public string Message;
        public bool Action;

        public IRCMessage(IRCChannel channel, IRCUser user, string message, bool action = false)
        {
            Channel = channel;
            User = user;
            Message = message;
            Action = action;
        }
    }
}
