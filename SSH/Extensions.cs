using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Security;
using System.Runtime.InteropServices;

namespace SSH
{
    static class Extensions
    {
        public static RNGCryptoServiceProvider Random = new RNGCryptoServiceProvider();

        public static byte[] PadLeft(this byte[] data, int len, byte b)
        {
            if (data.Length >= len) return data;
            return Enumerable.Repeat(b, len - data.Length).Concat(data).ToArray();
        }

        public static void Increment(this byte[] data)
        {
            for (int i = data.Length - 1; i >= 0 && ++data[i] == 0; i--) ;
        }

        public static void Xor(this byte[] d, byte[] data, int offset, int length)
        {
            for (int x = offset; x < offset + length; x++)
            {
                d[x] ^= data[x];
            }
        }

        public static void Xor(this byte[] d, int offset, byte[] data, int offset2, int length)
        {
            for (int x = offset; x < offset + length; x++, offset2++)
            {
                d[x] ^= data[offset2];
            }
        }

        public static string ToHex(this byte[] data, string join = "")
        {
            return string.Join(join, data.Select((b) => b.ToString("x").PadLeft(2, '0')));
        }

        public static byte[] ToByteArray(this string s)
        {
            return System.Text.Encoding.Default.GetBytes(s);
        }

        public static byte[] ToByteArray(this int i)
        {
            byte[] data = new byte[4];
            data[0] = (byte)(i >> 24);
            data[1] = (byte)((i & 0xFF0000) >> 16);
            data[2] = (byte)((i & 0xFF00) >> 8);
            data[3] = (byte)(i & 0xFF);
            return data;
        }

        public static byte[] TrimLeft(this byte[] data, byte b)
        {
            return data.SkipWhile(b1 => b1 == b).ToArray();
        }

        public static string ToHex(this byte[] data)
        {
            return data.ToHex(0, data.Length);
        }

        public static string ToHex(this byte[] data, int offset, int length, string separator = "")
        {
            if (data == null || data.Length == 0) return string.Empty;
            return string.Join(separator, data.Skip(offset).Take(length).Select(b => b.ToString("x").PadLeft(2, '0')));
        }

        public static string HexDump(this byte[] data, int perLine = 16, string prepend = "    ")
        {
            var totalLines = (data.Length / perLine) + Math.Min(1, data.Length % perLine);
            var lineLength = prepend.Length + 10 + (4 * perLine) + 4 + 2;
            var totalLength = totalLines * lineLength;

            var s = new StringBuilder(totalLength);
            for (int i = 0; i < data.Length; i += perLine)
            {
                int take = Math.Min(perLine, data.Length - i);
                s.AppendLine();
                s.Append(prepend);
                s.Append(i.ToString("x").PadLeft(8, '0') + "h: ");
                s.Append(data.ToHex(i, take, " "));
                s.Append(string.Empty.PadLeft(3 * (perLine - take), ' '));
                s.Append("    ");
                s.Append(data.GetPrintable(i, take));
            }
            var value = s.ToString();
            Debug.WriteLine(string.Format("Allocated {0} bytes; Used {1} bytes", totalLength, value.Length));
            return value;
        }

        public static string GetPrintable(this byte[] data, int offset, int length)
        {
            return string.Join("", data.Skip(offset).Take(length).Select(b => char.IsControl((char)b) ? '.' : (char)b));
        }

        public static string ToCSV(this string[] s)
        {
            return string.Join(",", s);
        }

        public static BigInteger AddUnsignedBigEndian(this BigInteger b, byte[] bytes)
        {
            var bytes2 = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, bytes2, 1, bytes.Length);
            return b + new BigInteger(bytes2.Reverse().ToArray());
        }

        public static byte[] GetBytes(this RNGCryptoServiceProvider rng, int length)
        {
            var random = new byte[length];
            rng.GetBytes(random);
            return random;
        }

        public static void ClearBytes(this RNGCryptoServiceProvider rng, byte[] data, int offset, int length)
        {
            var random = new byte[length];
            rng.GetBytes(random);
            Buffer.BlockCopy(random, 0, data, offset, length);
        }

        public static void Consume(this SecureString ss, Action<byte[]> method)
        {
            IntPtr bstr = IntPtr.Zero;
            byte[] workArray = null;
            try
            {
                bstr = Marshal.SecureStringToBSTR(ss);
                unsafe
                {
                    byte* bstrBytes = (byte*)bstr;
                    workArray = new byte[ss.Length];

                    for (int i = 0; i < workArray.Length; i++)
                    {
                        workArray[i] = *bstrBytes++; bstrBytes++;
                    }
                }
                method(workArray);
            }
            finally
            {
                ss.Dispose();
                var rng = new RNGCryptoServiceProvider();
                if (workArray != null)
                    rng.ClearBytes(workArray, 0, workArray.Length);
                if (bstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(bstr);
            }
        }
    }
}
