using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public class IRCChannel
    {
        public string Name = "";

        public string Topic = "";
        
        public List<IRCUser> Users = new List<IRCUser>();

        public bool LinkRead = false;

        public bool RecordSeen = false;

        public IRCUser GetUser(string name)
        {
            string nl = name.ToLower();
            if (nl.Contains('!'))
            {
                nl = nl.Substring(0, nl.IndexOf('!'));
            }
            foreach (IRCUser user in Users)
            {
                if (user.Name.ToLower() == nl)
                {
                    return user;
                }
            }
            return null;
        }
    }
}
