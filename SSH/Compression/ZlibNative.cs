using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace SSH.Compression
{
    internal static class ZLibNative
    {
        internal static readonly IntPtr ZNullPtr = (IntPtr)0;
#if LINUX
        public const string ZLibNativeDllName = "zlib.so";
#else
        public const string ZLibNativeDllName = "zlibwapi.dll";
#endif
        private const string Kernel32DllName = "kernel32.dll";
        public const string ZLibVersion = "1.2.8";
        public const int Deflate_DefaultWindowBits = -15;
        public const int Deflate_DefaultMemLevel = 8;

        [SecurityCritical]
        public static ZLibNative.ErrorCode CreateZLibStreamForDeflate(out ZLibNative.ZLibStreamHandle zLibStreamHandle)
        {
            return ZLibNative.CreateZLibStreamForDeflate(out zLibStreamHandle, ZLibNative.CompressionLevel.DefaultCompression, -15, 8, ZLibNative.CompressionStrategy.DefaultStrategy);
        }

        [SecurityCritical]
        public static ZLibNative.ErrorCode CreateZLibStreamForDeflate(out ZLibNative.ZLibStreamHandle zLibStreamHandle, ZLibNative.CompressionLevel level, int windowBits, int memLevel, ZLibNative.CompressionStrategy strategy)
        {
            zLibStreamHandle = new ZLibNative.ZLibStreamHandle();
            return zLibStreamHandle.DeflateInit2_(level, windowBits, memLevel, strategy);
        }

        [SecurityCritical]
        public static ZLibNative.ErrorCode CreateZLibStreamForInflate(out ZLibNative.ZLibStreamHandle zLibStreamHandle)
        {
            return ZLibNative.CreateZLibStreamForInflate(out zLibStreamHandle, -15);
        }

        [SecurityCritical]
        public static ZLibNative.ErrorCode CreateZLibStreamForInflate(out ZLibNative.ZLibStreamHandle zLibStreamHandle, int windowBits)
        {
            zLibStreamHandle = new ZLibNative.ZLibStreamHandle();
            return zLibStreamHandle.InflateInit2_(windowBits);
        }

        [SecurityCritical]
        public static int ZLibCompileFlags()
        {
            return ZLibNative.ZLibStreamHandle.ZLibCompileFlags();
        }

        public enum FlushCode
        {
            NoFlush,
            PartialFlush,
            SyncFlush,
            FullFlush,
            Finish,
            Block,
        }

        public enum ErrorCode
        {
            VersionError = -6,
            BufError = -5,
            MemError = -4,
            DataError = -3,
            StreamError = -2,
            ErrorNo = -1,
            Ok = 0,
            StreamEnd = 1,
            NeedDictionary = 2,
        }

        public enum CompressionLevel
        {
            DefaultCompression = -1,
            NoCompression = 0,
            BestSpeed = 1,
            BestCompression = 9,
        }

        public enum CompressionStrategy
        {
            DefaultStrategy,
            Filtered,
            HuffmanOnly,
            Rle,
            Fixed,
        }

        public enum CompressionMethod
        {
            Deflated = 8,
        }

        internal struct ZStream
        {
#pragma warning disable 0649
            internal IntPtr nextIn;
            internal uint availIn;
            internal uint totalIn;
            internal IntPtr nextOut;
            internal uint availOut;
            internal uint totalOut;
            internal IntPtr msg;
            internal IntPtr state;
            internal IntPtr zalloc;
            internal IntPtr zfree;
            internal IntPtr opaque;
            internal int dataType;
            internal uint adler;
            internal uint reserved;
#pragma warning restore 0649
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode DeflateInit2_Delegate(ref ZLibNative.ZStream stream, ZLibNative.CompressionLevel level, ZLibNative.CompressionMethod method, int windowBits, int memLevel, ZLibNative.CompressionStrategy strategy, [MarshalAs(UnmanagedType.LPStr)] string version, int streamSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode DeflateDelegate(ref ZLibNative.ZStream stream, ZLibNative.FlushCode flush);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode DeflateEndDelegate(ref ZLibNative.ZStream stream);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode InflateInit2_Delegate(ref ZLibNative.ZStream stream, int windowBits, [MarshalAs(UnmanagedType.LPStr)] string version, int streamSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode InflateDelegate(ref ZLibNative.ZStream stream, ZLibNative.FlushCode flush);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate ZLibNative.ErrorCode InflateEndDelegate(ref ZLibNative.ZStream stream);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        private delegate int ZlibCompileFlagsDelegate();

        private class NativeMethods
        {
#if LINUX
            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("libdl.so", EntryPoint = "dlopen")]
            private static extern ZLibNative.SafeLibraryHandle LoadLibrary(String libPath, int flags);
            internal static ZLibNative.SafeLibraryHandle LoadLibrary(String libPath)
            {
                return LoadLibrary(libPath, 0x02);
            }

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("libdl.so", EntryPoint = "dlsym")]
            internal static extern IntPtr GetProcAddress(ZLibNative.SafeLibraryHandle moduleHandle, String procName);

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("libdl.so", EntryPoint = "dlclose")]
            internal static extern bool FreeLibrary(IntPtr moduleHandle);
#else
            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern ZLibNative.SafeLibraryHandle LoadLibrary(string libPath);

            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false)]
            internal static extern IntPtr GetProcAddress(ZLibNative.SafeLibraryHandle moduleHandle, string procName);

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [SecurityCritical]
            [DllImport("kernel32.dll")]
            internal static extern bool FreeLibrary(IntPtr moduleHandle);
#endif
        }

        [SecurityCritical]
        private class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            internal SafeLibraryHandle()
              : base(true)
            {
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                int num = ZLibNative.NativeMethods.FreeLibrary(this.handle) ? 1 : 0;
                this.handle = IntPtr.Zero;
                return num != 0;
            }
        }

        [SecurityCritical]
        public sealed class ZLibStreamHandle : SafeHandleMinusOneIsInvalid
        {
            [SecurityCritical]
            private static ZLibNative.SafeLibraryHandle zlibLibraryHandle;
            private ZLibNative.ZStream zStream;
            [SecurityCritical]
            private volatile ZLibNative.ZLibStreamHandle.State initializationState;

            public ZLibNative.ZLibStreamHandle.State InitializationState
            {
                [SecurityCritical]
                get
                {
                    return this.initializationState;
                }
            }

            public IntPtr NextIn
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.nextIn;
                }
                [SecurityCritical]
                set
                {
                    this.zStream.nextIn = value;
                }
            }

            public uint AvailIn
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.availIn;
                }
                [SecurityCritical]
                set
                {
                    this.zStream.availIn = value;
                }
            }

            public uint TotalIn
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.totalIn;
                }
            }

            public IntPtr NextOut
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.nextOut;
                }
                [SecurityCritical]
                set
                {
                    this.zStream.nextOut = value;
                }
            }

            public uint AvailOut
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.availOut;
                }
                [SecurityCritical]
                set
                {
                    this.zStream.availOut = value;
                }
            }

            public uint TotalOut
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.totalOut;
                }
            }

            public int DataType
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.dataType;
                }
            }

            public uint Adler
            {
                [SecurityCritical]
                get
                {
                    return this.zStream.adler;
                }
            }

            public ZLibStreamHandle()
              : base(true)
            {
                this.zStream = new ZLibNative.ZStream();
                this.zStream.zalloc = ZLibNative.ZNullPtr;
                this.zStream.zfree = ZLibNative.ZNullPtr;
                this.zStream.opaque = ZLibNative.ZNullPtr;
                this.initializationState = ZLibNative.ZLibStreamHandle.State.NotInitialized;
                this.handle = IntPtr.Zero;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                if (ZLibNative.ZLibStreamHandle.zlibLibraryHandle == null || ZLibNative.ZLibStreamHandle.zlibLibraryHandle.IsInvalid)
                    return false;
                switch (this.InitializationState)
                {
                    case ZLibNative.ZLibStreamHandle.State.NotInitialized:
                        return true;
                    case ZLibNative.ZLibStreamHandle.State.InitializedForDeflate:
                        return this.DeflateEnd() == ZLibNative.ErrorCode.Ok;
                    case ZLibNative.ZLibStreamHandle.State.InitializedForInflate:
                        return this.InflateEnd() == ZLibNative.ErrorCode.Ok;
                    case ZLibNative.ZLibStreamHandle.State.Disposed:
                        return true;
                    default:
                        return false;
                }
            }

            [SecurityCritical]
            private void EnsureNotDisposed()
            {
                if (this.InitializationState == ZLibNative.ZLibStreamHandle.State.Disposed)
                    throw new ObjectDisposedException(this.GetType().Name);
            }

            [SecurityCritical]
            private void EnsureState(ZLibNative.ZLibStreamHandle.State requiredState)
            {
                if (this.InitializationState != requiredState)
                    throw new InvalidOperationException("InitializationState != " + requiredState.ToString());
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode DeflateInit2_(ZLibNative.CompressionLevel level, int windowBits, int memLevel, ZLibNative.CompressionStrategy strategy)
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.NotInitialized);
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                ZLibNative.ErrorCode errorCode;
                try
                {
                }
                finally
                {
                    errorCode = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateInit2_Delegate(ref this.zStream, level, ZLibNative.CompressionMethod.Deflated, windowBits, memLevel, strategy, "1.2.3", Marshal.SizeOf((object)this.zStream));
                    this.initializationState = ZLibNative.ZLibStreamHandle.State.InitializedForDeflate;
                    ZLibNative.ZLibStreamHandle.zlibLibraryHandle.DangerousAddRef(ref success);
                }
                return errorCode;
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode Deflate(ZLibNative.FlushCode flush)
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.InitializedForDeflate);
                return ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateDelegate(ref this.zStream, flush);
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode DeflateEnd()
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.InitializedForDeflate);
                RuntimeHelpers.PrepareConstrainedRegions();
                ZLibNative.ErrorCode errorCode;
                try
                {
                }
                finally
                {
                    errorCode = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateEndDelegate(ref this.zStream);
                    this.initializationState = ZLibNative.ZLibStreamHandle.State.Disposed;
                    ZLibNative.ZLibStreamHandle.zlibLibraryHandle.DangerousRelease();
                }
                return errorCode;
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode InflateInit2_(int windowBits)
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.NotInitialized);
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                ZLibNative.ErrorCode errorCode;
                try
                {
                }
                finally
                {
                    errorCode = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateInit2_Delegate(ref this.zStream, windowBits, "1.2.3", Marshal.SizeOf((object)this.zStream));
                    this.initializationState = ZLibNative.ZLibStreamHandle.State.InitializedForInflate;
                    ZLibNative.ZLibStreamHandle.zlibLibraryHandle.DangerousAddRef(ref success);
                }
                return errorCode;
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode Inflate(ZLibNative.FlushCode flush)
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.InitializedForInflate);
                return ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateDelegate(ref this.zStream, flush);
            }

            [SecurityCritical]
            public ZLibNative.ErrorCode InflateEnd()
            {
                this.EnsureNotDisposed();
                this.EnsureState(ZLibNative.ZLibStreamHandle.State.InitializedForInflate);
                RuntimeHelpers.PrepareConstrainedRegions();
                ZLibNative.ErrorCode errorCode;
                try
                {
                }
                finally
                {
                    errorCode = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateEndDelegate(ref this.zStream);
                    this.initializationState = ZLibNative.ZLibStreamHandle.State.Disposed;
                    ZLibNative.ZLibStreamHandle.zlibLibraryHandle.DangerousRelease();
                }
                return errorCode;
            }

            [SecurityCritical]
            public unsafe string GetErrorMessage()
            {
                if (ZLibNative.ZNullPtr.Equals((object)this.zStream.msg))
                    return string.Empty;
                return new string((sbyte*)(void*)this.zStream.msg);
            }

            [SecurityCritical]
            internal static int ZLibCompileFlags()
            {
                return ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.zlibCompileFlagsDelegate();
            }

            [SecurityCritical]
            private static class NativeZLibDLLStub
            {
                [SecurityCritical]
                internal static ZLibNative.DeflateInit2_Delegate deflateInit2_Delegate;
                [SecurityCritical]
                internal static ZLibNative.DeflateDelegate deflateDelegate;
                [SecurityCritical]
                internal static ZLibNative.DeflateEndDelegate deflateEndDelegate;
                [SecurityCritical]
                internal static ZLibNative.InflateInit2_Delegate inflateInit2_Delegate;
                [SecurityCritical]
                internal static ZLibNative.InflateDelegate inflateDelegate;
                [SecurityCritical]
                internal static ZLibNative.InflateEndDelegate inflateEndDelegate;
                [SecurityCritical]
                internal static ZLibNative.ZlibCompileFlagsDelegate zlibCompileFlagsDelegate;

                [SecuritySafeCritical]
                static NativeZLibDLLStub()
                {
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.LoadZLibDLL();
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.InitDelegates();
                }

                [SecuritySafeCritical]
                private static void LoadZLibDLL()
                {
                    new FileIOPermission(PermissionState.Unrestricted).Assert();
                    string str = Path.Combine(Environment.CurrentDirectory, ZLibNativeDllName);
                    if (!File.Exists(str))
                        throw new DllNotFoundException(ZLibNativeDllName);
                    ZLibNative.SafeLibraryHandle safeLibraryHandle = ZLibNative.NativeMethods.LoadLibrary(str);
                    if (safeLibraryHandle.IsInvalid)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error(), new IntPtr(-1));
                        throw new InvalidOperationException();
                    }
                    ZLibNative.ZLibStreamHandle.zlibLibraryHandle = safeLibraryHandle;
                }

                [SecurityCritical]
                private static DT CreateDelegate<DT>(string entryPointName)
                {
                    IntPtr procAddress = ZLibNative.NativeMethods.GetProcAddress(ZLibNative.ZLibStreamHandle.zlibLibraryHandle, entryPointName);
                    if (IntPtr.Zero == procAddress)
                        throw new EntryPointNotFoundException(string.Format("{0}!{1}", ZLibNativeDllName, entryPointName));
                    return (DT)((Object)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DT)));
                }

                [SecuritySafeCritical]
                private static void InitDelegates()
                {
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateInit2_Delegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.DeflateInit2_Delegate>("deflateInit2_");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateDelegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.DeflateDelegate>("deflate");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateEndDelegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.DeflateEndDelegate>("deflateEnd");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateInit2_Delegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.InflateInit2_Delegate>("inflateInit2_");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateDelegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.InflateDelegate>("inflate");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateEndDelegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.InflateEndDelegate>("inflateEnd");
                    ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.zlibCompileFlagsDelegate = ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.CreateDelegate<ZLibNative.ZlibCompileFlagsDelegate>("zlibCompileFlags");
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateInit2_Delegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateDelegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.deflateEndDelegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateInit2_Delegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateDelegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.inflateEndDelegate);
                    RuntimeHelpers.PrepareDelegate((Delegate)ZLibNative.ZLibStreamHandle.NativeZLibDLLStub.zlibCompileFlagsDelegate);
                }
            }

            public enum State
            {
                NotInitialized,
                InitializedForDeflate,
                InitializedForInflate,
                Disposed,
            }
        }
    }
}
