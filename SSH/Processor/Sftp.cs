using SSH.IO;
using SSH.Packets;
using SSH.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SSH.Processor
{
    public class Sftp : ChannelProcessor
    {
        internal Dictionary<uint, DataWaitHandle<SftpPacket>> WaitHandles { get { return requestWaitHandles; } }

        private EventWaitHandle waitHandle;
        private Dictionary<uint, DataWaitHandle<SftpPacket>> requestWaitHandles;

        private uint nextRequestId = 0;

        public Sftp(Session session)
            : base(session)
        {
            this.Type = ChannelType.Sftp;
            requestWaitHandles = new Dictionary<uint, DataWaitHandle<SftpPacket>>();
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            session.Socket.WritePacket(new SshChannelOpen("session", this.LocalChannel, 0xffffffff, 0x4000));
            waitHandle.WaitOne();
        }

        public override void Close()
        {
            session.Socket.WritePacket(new SshChannelClose(this.RemoteChannel));
            waitHandle.WaitOne();
            Close(SSH.StatusCode.OK);
        }

        private uint CreateRequestId()
        {
            return nextRequestId++;
        }

        internal DataWaitHandle<SftpPacket> CreateWaitHandle(out uint requestId)
        {
            requestId = CreateRequestId();
            var waitHandle = new DataWaitHandle<SftpPacket>(false, EventResetMode.AutoReset);
            requestWaitHandles.Add(requestId, waitHandle);
            return requestWaitHandles[requestId];
        }

        internal void DestroyWaitHandle(uint requestId)
        {
            requestWaitHandles.Remove(requestId);
        }

        private bool ProcessPacket(SftpPacket p)
        {
            switch (p.SftpCode)
            {
                case SftpMessageCode.SSH_FXP_DATA:
                    {
                        var msg = (SftpData)p;
                        WaitHandles[msg.RequestId].Set(msg);
                        return true;
                    }
                case SftpMessageCode.SSH_FXP_VERSION:
                    waitHandle.Set();
                    return true;
                case SftpMessageCode.SSH_FXP_HANDLE:
                    {
                        var msg = (SftpHandle)p;
                        WaitHandles[msg.RequestId].Set(msg);
                        return true;
                    }
                case SftpMessageCode.SSH_FXP_NAME:
                    {
                        var msg = (SftpName)p;
                        WaitHandles[msg.RequestId].Set(msg);
                        return true;
                    }
                case SftpMessageCode.SSH_FXP_ATTRS:
                    {
                        var msg = (SftpAttrs)p;
                        WaitHandles[msg.RequestId].Set(msg);
                        return true;
                    }
                case SftpMessageCode.SSH_FXP_STATUS:
                    {
                        var msg = (SftpStatus)p;
                        switch (msg.Status)
                        {
                            case SftpStatusCode.SSH_FX_NO_SUCH_FILE:
                                WaitHandles[msg.RequestId].Set(new FileNotFoundException(msg.Message));
                                return true;
                            case SftpStatusCode.SSH_FX_PERMISSION_DENIED:
                            case SftpStatusCode.SSH_FX_FAILURE:
                                WaitHandles[msg.RequestId].Set(new IOException(msg.Message));
                                return true;
                            case SftpStatusCode.SSH_FX_OK:
                                WaitHandles[msg.RequestId].Set();
                                return true;
                            case SftpStatusCode.SSH_FX_EOF:
                                WaitHandles[msg.RequestId].Set();
                                return true;
                        }
                        return false;
                    }
            }
            return false;
        }

        public SftpFileInfo GetFileInfo(string path)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpStat(this.RemoteChannel, requestId, path));
            try
            {
                waitHandle.WaitOne();
                var fi = ((SftpAttrs)waitHandle.Result).Attrs;
                fi.DirectoryName = Path.GetDirectoryName(path);
                fi.Name = Path.GetFileName(path);
                return fi;
            }
            finally
            {
                DestroyWaitHandle(requestId);
            }
        }

        public bool Exists(string path)
        {
            try
            {
                GetFileInfo(path);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            return true;
        }

        public string[] GetFiles(string path)
        {
            return GetFiles(path, fi => fi.IsRegularFile || fi.IsSymbolicLink);
        }

        public string[] GetDirectories(string path)
        {
            return GetFiles(path, fi => fi.IsDirectory && fi.Name != "." && fi.Name != "..");
        }

        public string[] GetFiles(string path, params Func<SftpFileInfo, bool>[] filters)
        {
            var filesInfo = GetFilesInfo(path, filters);
            var files = filesInfo.Select(fi => fi.FullName).ToArray();
            return files;
        }

        public SftpFileInfo[] GetFilesInfo(string path)
        {
            return GetFilesInfo(path, fi => fi.IsRegularFile || fi.IsSymbolicLink);
        }

        public SftpFileInfo[] GetDirectoriesInfo(string path)
        {
            return GetFilesInfo(path, fi => fi.IsDirectory);
        }

        public SftpFileInfo[] GetFilesInfo(string path, params Func<SftpFileInfo, bool>[] filters)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            var handle = OpenDirectory(path);
            List<SftpName> names = new List<SftpName>();

            session.Socket.WritePacket(new SftpReadDirectory(this.RemoteChannel, requestId, handle));
            waitHandle.WaitOne();
            while (waitHandle.Result != null)
            {
                var ns = (SftpName)waitHandle.Result;
                foreach (var name in ns.Files)
                    name.DirectoryName = path;
                names.Add(ns);
                session.Socket.WritePacket(new SftpReadDirectory(this.RemoteChannel, requestId, handle));
                waitHandle.WaitOne();
            }

            var files = names.SelectMany(p => p.Files).
                Where(fi => filters.Any(filter => filter(fi))).
                OrderBy(fi => fi.Name).ToArray();
            DestroyWaitHandle(requestId);
            CloseHandle(handle);
            return files;
        }

        internal void CloseHandle(byte[] handle)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpClose(this.RemoteChannel, requestId, handle));
            waitHandle.WaitOne();
            DestroyWaitHandle(requestId);
        }

        public void Rename(string source, string target)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpRename(this.RemoteChannel, requestId, source, target));
            waitHandle.WaitOne();
            DestroyWaitHandle(requestId);
        }

        public void DeleteFile(string path)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpRemove(this.RemoteChannel, requestId, path));
            waitHandle.WaitOne();
            DestroyWaitHandle(requestId);
        }

        public void DeleteDirectory(string path)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpRemoveDir(this.RemoteChannel, requestId, path));
            waitHandle.WaitOne();
            DestroyWaitHandle(requestId);
        }

        public void CreateDirectory(string path, string permissions = "")
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpMakeDir(this.RemoteChannel, requestId, path, permissions));
            waitHandle.WaitOne();
            DestroyWaitHandle(requestId);
        }

        private byte[] OpenDirectory(string path)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            session.Socket.WritePacket(new SftpOpenDirectory(this.RemoteChannel, requestId, path));
            waitHandle.WaitOne();
            var handle = ((SftpHandle)waitHandle.Result).Handle;
            DestroyWaitHandle(requestId);
            return handle;
        }

        public Stream Open(string path, FileMode fileMode)
        {
            try
            {
                return Open(path, fileMode, "", null);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }

        public Stream Open(string path, FileMode fileMode, string permissions)
        {
            try
            {
                return Open(path, fileMode, permissions, null);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
        }

        public Stream Open(string path, FileMode fileMode, string permissions, ulong? bytesToRead)
        {
            uint requestId;
            var waitHandle = CreateWaitHandle(out requestId);
            Stream s;

            try
            {
                switch (fileMode)
                {
                    case FileMode.OpenOrCreate:
                    case FileMode.Append:
                        {
                            try
                            {
                                var fi = GetFileInfo(path);
                                session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_WRITE | FileModeFlags.SSH_FXF_APPEND, permissions));
                                waitHandle.WaitOne();
                                var handle = ((SftpHandle)waitHandle.Result).Handle;
                                s = new SftpStream(this, handle);
                                ((SftpStream)s).SetLength(s.Position = (long)fi.Length);
                            }
                            catch (FileNotFoundException)
                            {
                                session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_CREAT | FileModeFlags.SSH_FXF_WRITE, permissions));
                                waitHandle.WaitOne();
                                var handle = ((SftpHandle)waitHandle.Result).Handle;
                                s = new SftpStream(this, handle);
                            }
                        }
                        break;
                    case FileMode.Create:
                        {
                            session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_CREAT | FileModeFlags.SSH_FXF_WRITE, permissions));
                            waitHandle.WaitOne();
                            var handle = ((SftpHandle)waitHandle.Result).Handle;
                            s = new SftpStream(this, handle);
                        }
                        break;
                    case FileMode.CreateNew:
                        {
                            session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_CREAT | FileModeFlags.SSH_FXF_WRITE | FileModeFlags.SSH_FXF_EXCL, permissions));
                            waitHandle.WaitOne();
                            var handle = ((SftpHandle)waitHandle.Result).Handle;
                            s = new SftpStream(this, handle);
                        }
                        break;
                    case FileMode.Open:
                        {
                            var length = bytesToRead ?? GetFileInfo(path).Length;
                            session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_READ, permissions));
                            waitHandle.WaitOne();
                            var handle = ((SftpHandle)waitHandle.Result).Handle;
                            s = new SftpStream(this, handle, length);
                        }
                        break;
                    case FileMode.Truncate:
                        {
                            session.Socket.WritePacket(new SftpOpen(this.RemoteChannel, requestId, path, FileModeFlags.SSH_FXF_CREAT | FileModeFlags.SSH_FXF_TRUNC | FileModeFlags.SSH_FXF_WRITE, permissions));
                            waitHandle.WaitOne();
                            var handle = ((SftpHandle)waitHandle.Result).Handle;
                            s = new SftpStream(this, handle);
                        }
                        break;
                    default:
                        s = Stream.Null;
                        break;
                }
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            finally
            {
                DestroyWaitHandle(requestId);
            }
            return s;
        }

        public override void OnChannelOpenConfirmation(ISshChannelMessage p)
        {
            var msg = (SshChannelOpenConfirmation)p;
            this.RemoteChannel = msg.SenderChannel;
            session.Socket.WritePacket(new SshChannelRequestSftp(this.RemoteChannel));
        }

        public override void OnChannelOpenFailure(ISshChannelMessage p) { }

        public override void OnChannelSuccess(ISshChannelMessage p)
        {
            session.Socket.WritePacket(new SftpInit(this.RemoteChannel, 3));
        }

        public override void OnChannelWindowAdjust(ISshChannelMessage p) { }

        public override void OnChannelData(ISshChannelMessage p)
        {
            ProcessPacket((SftpPacket)p);
        }

        public override void OnChannelExtendedData(ISshChannelMessage p) { }

        public override void OnChannelClose(ISshChannelMessage p)
        {
            waitHandle.Set();
        }

        public override void OnChannelEndOfFile(ISshChannelMessage p) { }

        public override void OnChannelRequest(ISshChannelMessage p) { }
    }
}
