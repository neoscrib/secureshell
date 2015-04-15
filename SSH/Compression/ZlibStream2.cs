using System.IO;
using System.IO.Compression;

namespace SSH.Compression
{
    public class ZlibStream2 : DeflateStream
    {
        int s1 = 1;
        int s2 = 0;

        private CompressionMode mode;
        private long position = 0;

        public override long Position
        {
            get
            {
                return mode == CompressionMode.Compress ? base.Position : position;
            }
            set
            {
                base.Position = value;
            }
        }

        public override long Length
        {
            get
            {
                return mode == CompressionMode.Compress ? base.Length : base.Length - 6;
            }
        }

        public ZlibStream2(Stream stream, CompressionMode mode)
            : base(stream, mode, true)
        {
            this.mode = mode;
            if (mode == CompressionMode.Compress)
            {
                stream.Write(new byte[] { 120, 1 }, 0, 2);
                // 0111 (CINFO, 32K window size), 1000 (CM, deflate)
                // 00 (FLEVEL, 0), 0 (FDICT, None), 00001 (FCHECK)
            }
            else
            {
                stream.Position += 2;
            }
        }

        public override int ReadByte()
        {
            if (base.Position >= base.Length - 4)
                return -1;
            return base.ReadByte();
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var read = base.Read(array, offset, count);
            position += read;
            return read;
        }

        public override void Write(byte[] array, int offset, int count)
        {
            for (int i = offset; i < count; i++)
            {
                s1 = (s1 + array[i]) % 65521;
                s2 = (s2 + s1) % 65521;
            }
            base.Write(array, offset, count);
        }

        public override void Close()
        {
            var stream = BaseStream;
            base.Close();

            if (mode == CompressionMode.Compress)
            {
                int adler32 = s2 * 65536 + s1;
                byte[] buffer = new byte[4];
                buffer[0] = (byte)(adler32 >> 24);
                buffer[1] = (byte)(adler32 >> 16);
                buffer[2] = (byte)(adler32 >> 8);
                buffer[3] = (byte)adler32;
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();
            }
        }
    }
}
