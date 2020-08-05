using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkBot
{
    public static class Extensions
    {
        public static bool OlderThan(this DateTime dateTime, int minutes)
        {
            if (DateTime.Now - dateTime < new TimeSpan(0, minutes, 0))
                return true;
            return false;
        }

        public static string AsString(this IEnumerable<char> chars) => new string(chars.ToArray());
    }
}