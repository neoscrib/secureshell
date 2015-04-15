using System;
using System.IO;
using SSH.Packets;
using SSH.Processor;

namespace SSH.IO
{
    public class SftpStream : Stream
    {
        private bool reader = false;
        private ulong length;
        private long length2;
        private Sftp sftp;
        private byte[] handle;

        public override bool CanRead { get { return reader; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return !reader; } }
        public override long Length { get { return length2; } }

        private bool closeSent = false;

        /// <summary>
        /// Create a stream for writing to an SFTP resource.
        /// </summary>
        /// <param name="sftp"></param>
        /// <param name="channel"></param>
        /// <param name="handle"></param>
        internal SftpStream(Sftp sftp, byte[] handle)
            : base()
        {
            this.sftp = sftp;
            this.handle = handle;
            this.reader = false;
        }

        /// <summary>
        /// Creates a stream for reading from an SFTP resource.
        /// </summary>
        /// <param name="sftp"></param>
        /// <param name="channel"></param>
        /// <param name="handle"></param>
        /// <param name="length"></param>
        public SftpStream(Sftp sftp, byte[] handle, ulong length)
            : base()
        {
            this.sftp = sftp;
            this.handle = handle;
            this.length2 = (long)(this.length = length);
            this.reader = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CanRead)
            {
                uint requestId;
                var waitHandle = sftp.CreateWaitHandle(out requestId);
                sftp.session.Socket.WritePacket(new SftpRead(sftp.RemoteChannel, requestId, handle, Convert.ToUInt64(Position), Convert.ToUInt32(count)));
                waitHandle.WaitOne();
                byte[] data = new byte[0];
                if (waitHandle.Result != null)
                {
                    data = ((SftpData)waitHandle.Result).Data;
                    Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
                    Position += data.Length;
                }
                sftp.DestroyWaitHandle(requestId);
                return data.Length;
            }
            else
            {
                throw new IOException("Stream cannot be read from.");
            }
        }

        public override void Close()
        {
            if (!closeSent)
                sftp.CloseHandle(handle);
            closeSent = true;
        }

        public override void Flush() { }

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.Current:
                case SeekOrigin.End: Position += offset; break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            if (!CanRead && CanWrite)
                this.length = (ulong)(this.length2 = value);
            else
                throw new IOException("Can't set length on a read-only stream.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CanWrite)
            {
                uint requestId;
                var waitHandle = sftp.CreateWaitHandle(out requestId);
                var data = new byte[count];
                Buffer.BlockCopy(buffer, 0, data, 0, count);
                sftp.session.Socket.WritePacket(new SftpWrite(sftp.RemoteChannel, requestId, handle, Convert.ToUInt64(Position), data));
                waitHandle.WaitOne();
                Position += count;
                if (length2 < Position)
                    length = (ulong)(length2 = Position);
                sftp.DestroyWaitHandle(requestId);
            }
            else
            {
                throw new IOException("Stream cannot be written to.");
            }
        }
    }
}