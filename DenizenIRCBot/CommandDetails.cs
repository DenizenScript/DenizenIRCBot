using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public class CommandDetails
    {
        public string Name;

        public List<string> Arguments = new List<string>();

        public string Pinger;

        public IRCUser User;

        public IRCChannel Channel;
    }
}
