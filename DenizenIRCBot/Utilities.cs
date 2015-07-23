﻿using System;
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

        public static int StringToInt(string input)
        {
            int outp;
            if (int.TryParse(input, out outp))
            {
                return outp;
            }
            return 0;
        }

        public static double StringToDouble(string input)
        {
            double outp;
            if (double.TryParse(input, out outp))
            {
                return outp;
            }
            return 0;
        }

        public static string FormatNumber(int input)
        {
            if (input < 10 && input >= 0)
            {
                return "0" + input;
            }
            return input.ToString();
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

        public static TimeSpan StringToDuration(string orig)
        {
            if (orig.Length <= 1)
            {
                return new TimeSpan(0, 0, StringToInt(orig));
            }
            float mult = 1;
            bool hassuffix = true;
            if (orig.EndsWith("t"))
            {
                mult = 1 / 20;
            }
            else if (orig.EndsWith("s"))
            {
                mult = 1;
            }
            else if (orig.EndsWith("m"))
            {
                mult = 60;
            }
            else if (orig.EndsWith("h"))
            {
                mult = 60 * 60;
            }
            else if (orig.EndsWith("d"))
            {
                mult = 60 * 60 * 24;
            }
            else if (orig.EndsWith("w"))
            {
                mult = 60 * 60 * 24 * 7;
            }
            else
            {
                hassuffix = false;
            }
            if (hassuffix)
            {
                orig = orig.Substring(0, orig.Length - 1);
            }
            return new TimeSpan(0, 0, (int)(StringToDouble(orig) * mult));
        }

        public static string DateToString(DateTime date)
        {
            date = date.ToUniversalTime();
            return date.Year + "-" + FormatNumber(date.Month) + "-" + FormatNumber(date.Day) + "-" + FormatNumber(date.Hour) + "-" + FormatNumber(date.Minute) + "-" + FormatNumber(date.Second);
        }

        public static string FormatDate(DateTime dt)
        {
            string utcoffset = "";
            DateTime UTC = dt.ToUniversalTime();
            if (dt.CompareTo(UTC) < 0)
            {
                TimeSpan span = UTC.Subtract(dt);
                utcoffset = "-" + FormatNumber(((int)Math.Floor(span.TotalHours))) + ":" + FormatNumber(span.Minutes);
            }
            else
            {
                TimeSpan span = dt.Subtract(UTC);
                utcoffset = "+" + FormatNumber(((int)Math.Floor(span.TotalHours))) + ":" + FormatNumber(span.Minutes);
            }
            return dt.Year + "/" + FormatNumber(dt.Month) + "/" + FormatNumber(dt.Day) + " " + FormatNumber(dt.Hour) + ":" + FormatNumber(dt.Minute) + ":" + FormatNumber(dt.Second) + " UTC" + utcoffset;
        }

        public static string FormatDateRelative(DateTime dt)
        {
            return FormatDate(dt) + " (" + tstostr(DateTime.UtcNow.ToFileTimeUtc() - dt.ToFileTimeUtc()) + " ago)";
        }

        public static string tstostr(long input)
        {
            long sec = (long)(input / 10000000);
            long min = sec / 60;
            sec = sec - (min * 60);
            long hour = min / 60;
            min = min - (hour * 60);
            long day = hour / 24;
            hour = hour - (day * 24);
            long year = day / 365;
            day = day - (year * 365);
            if (year != 0)
            {
                return year + " year" + (Math.Abs(year) == 1 ? "" : "s") + " and " + day + " day" + (Math.Abs(day) == 1 ? "" : "s");
            }
            else if (day != 0)
            {
                return day + " day" + (Math.Abs(day) == 1 ? "" : "s") + " and " + hour + " hour" + (Math.Abs(hour) == 1 ? "" : "s");
            }
            else if (hour != 0)
            {
                return hour + " hour" + (Math.Abs(hour) == 1 ? "" : "s") + " and " + min + " minute" + (Math.Abs(min) == 1 ? "" : "s");
            }
            else if (min != 0)
            {
                return min + " minute" + (Math.Abs(min) == 1 ? "" : "s") + " and " + sec + " second" + (Math.Abs(sec) == 1 ? "" : "s");
            }
            else
            {
                return sec + " second" + (Math.Abs(sec) == 1 ? "" : "s");
            }
        }

        public static DateTime StringToDate(string input)
        {
            string[] dat = input.Split('-');
            int year = StringToInt(dat[0]);
            int month = StringToInt(dat[1]);
            int day = StringToInt(dat[2]);
            int hour = StringToInt(dat[3]);
            int minute = StringToInt(dat[4]);
            int second = StringToInt(dat[5]);
            return new DateTime(year, month, day, hour, minute, second, 0, DateTimeKind.Utc);
        }
    }
}
