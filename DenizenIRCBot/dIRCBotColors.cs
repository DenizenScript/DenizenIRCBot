using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public const char C_S_BOLD = (char)0x02;
        public const char C_S_COLOR = (char)0x03;
        public const char C_S_NORMAL = (char)0x0F;
        public const char C_S_UNDERLINE = (char)0x1F;
        public static string S_UNDERLINE = C_S_UNDERLINE.ToString();
        public static string S_NORMAL = C_S_NORMAL.ToString();
        public static string S_BOLD = C_S_BOLD.ToString();
        public static string S_WHITE = C_S_COLOR.ToString() + "00";
        static string S_BLACK = C_S_COLOR.ToString() + "01";
        static string S_DARKBLUE = C_S_COLOR.ToString() + "02";
        static string S_GREEN = C_S_COLOR.ToString() + "03";
        static string S_RED = C_S_COLOR.ToString() + "04";
        static string S_BROWN = C_S_COLOR.ToString() + "05";
        static string S_PURPLE = C_S_COLOR.ToString() + "06";
        static string S_ORANGE = C_S_COLOR.ToString() + "07";
        static string S_YELLOW = C_S_COLOR.ToString() + "08";
        static string S_LIME = C_S_COLOR.ToString() + "09";
        static string S_CYAN = C_S_COLOR.ToString() + "10";
        static string S_BLUE = C_S_COLOR.ToString() + "11";
        static string S_DARKCYAN = C_S_COLOR.ToString() + "12";
        static string S_MAGENTA = C_S_COLOR.ToString() + "13";
        static string S_DARKGRAY = C_S_COLOR.ToString() + "14";
        static string S_GRAY = C_S_COLOR.ToString() + "15";
        public static char actionchr = (char)0x01;

        public string ColorGeneral = S_NORMAL + S_RED;
        public string ColorHighlightMinor = S_BROWN;
        public string ColorHighlightMajor = S_ORANGE;
        public string ColorLink = S_DARKBLUE;
    }
}
