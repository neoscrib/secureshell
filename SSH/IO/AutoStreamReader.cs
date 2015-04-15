/*
 *  Copyright (c) 2012-2013 Synapse Product Development. All rights reserved.
 */

using System.IO;
using System.Text;

namespace SSH.IO
{
    /// <summary>
    /// Provides convenience methods for reading data from streams.
    /// </summary>
    public static class AutoStreamReader
    {
        /// <summary>
        /// Reads the specified Stream to the end and returns the data as a String.
        /// </summary>
        /// <param name="s">The Stream from which to read.</param>
        /// <returns>A String containing the data read from the Stream.</returns>
        public static string ReadToEnd(Stream s)
        {
            var sw = new StreamReader(s);
            return sw.ReadToEnd();
        }

        public static string ReadLine(Stream s)
        {
            var sb = new StringBuilder();
            var b = s.ReadByte();
            while (b > -1 && b != '\r' && b != '\n')
            {
                sb.Append(Encoding.Default.GetString(new[] { (byte)b }));
                b = s.ReadByte();
            }
            if (b == '\r') s.ReadByte();
            return sb.ToString();
        }
    }
}
