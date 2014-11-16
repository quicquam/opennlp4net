﻿
namespace opennlp.nonjava.helperclasses
{
    internal static class DateTimeHelperClass
    {
        private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0,
            System.DateTimeKind.Utc);

        internal static long CurrentUnixTimeMillis()
        {
            return (long) (System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}