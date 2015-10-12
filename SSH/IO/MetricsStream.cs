using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSH.IO
{
    class MetricsStream : Stream
    {
        public ulong BytesRead { get; set; }
        public ulong BytesWritten { get; set; }
        public Stream BaseStream { get; set; }

        public MetricsStream(Stream baseStream) : base()
        {
            this.BaseStream = baseStream;
        }

        public override bool CanRead
        {
            get
            {
                return BaseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return BaseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return BaseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return BaseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return BaseStream.Position;
            }

            set
            {
                BaseStream.Position = value;
            }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = BaseStream.Read(buffer, offset, count);
            BytesRead += (uint)read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BytesWritten += (uint)count;
            BaseStream.Write(buffer, offset, count);
        }
    }
}
