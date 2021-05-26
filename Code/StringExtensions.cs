using System;
using System.Collections.Generic;
using System.Linq;

namespace Saved.Code
{
    public static class StringExtension
    {

        public static bool IsNullOrWhitespace(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable<string>()
                .Select(s => s.Trim())
                .ToList();
        }
        public static string Left(this string o, int oLength)
        {
            if (o.Length < oLength)
            {
                return o;
            }
            return o.Substring(0, oLength);
        }
        public static bool IsNullOrEmpty(this string str)
        {
            if (str == null || str == String.Empty)
                return true;
            return false;
        }

        public static string TrimAndReduce(this string str)
        {
            return str.Trim();
        }

        public static string ToNonNullString(this object o)
        {
            if (o == null)
                return String.Empty;
            return o.ToString();
        }

        public static string[] Split(this string str, string sDelimiter)
        {
            string[] vSplitData = str.Split(new string[] { sDelimiter }, StringSplitOptions.None);
            return vSplitData;
        }

        public static double ToDouble(this string o)
        {
            try
            {
                if (o == null)
                    return 0;
                if (o.ToString() == string.Empty)
                    return 0;
                double d = Convert.ToDouble(o.ToString());
                return d;
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}
