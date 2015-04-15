using System;

namespace SSH
{
    /// <summary>
    /// A utility class for doing various things.
    /// </summary>
    public static class Util
    {
        static DateTime Jan1_1970 = new DateTime(1970, 1, 1);

        /// <summary>
        /// Converts a unix epoch date to a DateTime object.
        /// </summary>
        /// <param name="epoch">The unix epoch date (milliseconds from Jan 1, 1970).</param>
        /// <returns>A DateTime object representing the specified unix epoch date.</returns>
        public static DateTime ConvertEpoch(double epoch)
        {
            return Jan1_1970.AddMilliseconds(epoch);
        }

        /// <summary>
        /// Gets the current unix epoch date (milliseconds since Jan 1, 1970).
        /// </summary>
        /// <returns>A double representing the current unix epoch date (milliseconds since Jan 1, 1970).</returns>
        public static double GetEpoch()
        {
            return (DateTime.Now - Jan1_1970).TotalMilliseconds;
        }

        public static string ToHumanReadableSize(double n)
        {
            if (double.IsInfinity(n) || n == 0.0d) return "0 bytes";
            string[] specifiers = new string[] { "bytes", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };
            var o = Math.Abs(n);
            var index = (int)Math.Floor(Math.Log(o, 1024));
            o /= Math.Pow(1024, index);
            return string.Format("{0:0.000} {1}", o, specifiers[index]);
        }
    }
}
