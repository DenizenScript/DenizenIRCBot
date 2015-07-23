using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public static class Utilities
    {
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
    }
}
