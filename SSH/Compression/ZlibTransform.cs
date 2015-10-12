using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSH.Compression
{
    public sealed class ZlibTransform : ICryptoTransform
    {
        public CompressionMode Mode { get; private set; }
        public uint TotalIn { get { return handle.TotalIn; } }
        public ulong TotalOut { get { return handle.TotalOut; } }
        public double Ratio
        {
            get
            {
                if (Mode == CompressionMode.Compress)
                    return (double)TotalOut / (double)TotalIn;
                else
                    return (double)TotalIn / (double)TotalOut;
            }
        }

        private ZLibNative.ZLibStreamHandle handle;

        public ZlibTransform(CompressionMode mode)
        {
            this.Mode = mode;
            switch (mode)
            {
                case CompressionMode.Decompress:
                    ZLibNative.CreateZLibStreamForInflate(out handle, -15);
                    break;
                case CompressionMode.Compress:
                    ZLibNative.CreateZLibStreamForDeflate(out handle);
                    break;
            }
        }

        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }

        public int InputBlockSize
        {
            get
            {
                return 1;
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return 1;
            }
        }

        public void Dispose()
        {
            handle.Close();
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (TotalIn == 0 && Mode == CompressionMode.Decompress && inputBuffer[inputOffset] == 0x78 && inputBuffer[inputOffset + 1] == 0x9c)
            {
                inputOffset += 2;
                inputCount -= 2;
            }

            var read = 0;
            if (TotalOut == 0 && Mode == CompressionMode.Compress)
            {
                outputBuffer[outputOffset] = 0x78;
                outputBuffer[outputOffset + 1] = 0x9c;
                outputOffset += 2;
                read += 2;
            }

            var inputBufferHandle = new GCHandle?(GCHandle.Alloc((object)inputBuffer, GCHandleType.Pinned));
            handle.NextIn = inputBufferHandle.Value.AddrOfPinnedObject() + inputOffset;
            handle.AvailIn = (uint)inputCount;
            var outputBufferHandle = new GCHandle?(GCHandle.Alloc((object)outputBuffer, GCHandleType.Pinned));
            handle.NextOut = outputBufferHandle.Value.AddrOfPinnedObject() + outputOffset;
            handle.AvailOut = (uint)(outputBuffer.Length - outputOffset);
            ZLibNative.ErrorCode code;
            if (Mode == CompressionMode.Decompress)
                code = handle.Inflate(ZLibNative.FlushCode.PartialFlush);
            else
                code = handle.Deflate(ZLibNative.FlushCode.PartialFlush);

            read += (outputBuffer.Length - outputOffset) - (int)handle.AvailOut;
            return read;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var outputBuffer = new byte[65536];
            var length = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
            if (this.Mode == CompressionMode.Compress)
            {
                uint adler32 = handle.Adler;
                byte[] buffer = new byte[4];
                buffer[0] = (byte)(adler32 >> 24);
                buffer[1] = (byte)(adler32 >> 16);
                buffer[2] = (byte)(adler32 >> 8);
                buffer[3] = (byte)adler32;
                Buffer.BlockCopy(buffer, 0, outputBuffer, length, 4);
                length += 4;
            }

            var returnVal = new byte[length];
            Buffer.BlockCopy(outputBuffer, 0, returnVal, 0, length);
            return returnVal;
        }
    }
}
