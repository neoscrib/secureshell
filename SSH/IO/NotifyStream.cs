using System;
using System.IO;

namespace SSH.IO
{
    /// <summary>
    /// Represents a two-way Stream that notifies a listener when data has been written to the Stream.
    /// </summary>
    public class NotifyStream : Stream
    {
        /// <summary>
        /// The event raised when data is written to the Stream and is available for reading.
        /// </summary>
        public event EventHandler DataReceived;

        private byte[] data = new byte[32768];
        private long readPosition = 0;
        private long writePosition = 0;

        /// <summary>
        /// The number of bytes available for reading.
        /// </summary>
        public long BytesAvailable
        {
            get { return writePosition - readPosition; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush() { }

        public override long Length
        {
            get { return writePosition; }
        }

        public override long Position
        {
            get { return readPosition; }
            set { readPosition = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long bytesToRead = Math.Min(count, writePosition - readPosition);
            Buffer.BlockCopy(data, (int)readPosition, buffer, offset, (int)bytesToRead);
            readPosition += bytesToRead;
            return (int)bytesToRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    readPosition = offset;
                    break;
                case SeekOrigin.Current:
                    readPosition += offset;
                    break;
                case SeekOrigin.End:
                    readPosition = writePosition + offset;
                    break;
            }
            return readPosition;
        }

        public long SeekWrite(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    writePosition = offset;
                    break;
                case SeekOrigin.Current:
                    writePosition += offset;
                    break;
                case SeekOrigin.End:
                    writePosition = writePosition + offset;
                    break;
            }
            readPosition = writePosition;
            return writePosition;
        }

        internal void SetInternalLength(long value)
        {
            writePosition = value;
        }

        public override void SetLength(long value)
        {
            Array.Resize(ref data, (int)value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CanWrite)
            {
                if (writePosition + count > data.Length)
                {
                    long newsize = Math.Max(data.Length + 32768, writePosition + count);
                    SetLength(newsize);
                }
                Buffer.BlockCopy(buffer, offset, data, (int)writePosition, count);
                writePosition += count;
                if (DataReceived != null)
                    DataReceived(this, EventArgs.Empty);
            }
            else
            {
                throw new IOException("Stream cannot be written to.");
            }
        }
    }
}
