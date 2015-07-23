using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public static class Utilities
    {
        public static Random random = new Random();

        /// <summary>
        /// Parses a string to a ushort. Returns 0 if input is invalid.
        /// </summary>
        public static ushort StringToUShort(string input)
        {
            ushort outp;
            if (ushort.TryParse(input, out outp))
            {
                return outp;
            }
            return 0;
        }

        /// <summary>
        /// Concatecenates a list of strings.
        /// </summary>
        public static string Concat(List<string> input, int start = 0)
        {
            StringBuilder outp = new StringBuilder();
            for (int i = start; i < input.Count; i++)
            {
                outp.Append(input[i]);
                if (i + 1 < input.Count)
                {
                    outp.Append(" ");
                }
            }
            return outp.ToString();
        }
    }
}
