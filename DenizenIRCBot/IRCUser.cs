using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public class IRCUser
    {
        public IRCUser(string mask)
        {
            ParseMask(mask);
        }

        public void ParseMask(string mask)
        {
            if (mask.StartsWith("@"))
            {
                OP = true;
                mask = mask.Substring(1);
            }
            if (mask.StartsWith("+"))
            {
                Voice = true;
                EverHadVoice = true;
                mask = mask.Substring(1);
            }
            OriginalMask = mask;
            int s1 = mask.IndexOf('!');
            int s2 = mask.IndexOf('@');
            Name = mask.Substring(0, s1);
            Ident = mask.Substring(s1 + 1, (s2 - s1) - 1);
            IP = mask.Substring(s2 + 1);
        }

        public bool OP;

        public bool Voice;

        public string OriginalMask;

        public string Name;

        public string Ident;

        public string IP;

        public bool EverHadVoice = false;
    }
}
