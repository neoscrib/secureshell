using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SSH.Processor;

namespace SSH
{
    public class DirectTcpIp : IDisposable
    {
        private TcpListener listener;
        private Thread thread;
        private string remoteAddress;
        private uint remotePort;
        private Session session;

        public DirectTcpIp(Session session, IPEndPoint local, string remoteAddress, uint remotePort)
        {
            session.Disposables.Add(this);
            this.session = session;
            this.remoteAddress = remoteAddress;
            this.remotePort = remotePort;
            this.listener = new TcpListener(local);
            this.thread = new Thread(new ThreadStart(ListenerThread));
            this.thread.Start();
        }

        public void Stop()
        {
            this.listener.Stop();
        }

        private void ListenerThread()
        {
            try
            {
                this.listener.Start();
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    client.Client.NoDelay = true;
                    new LocalForward(this.session, client, this.remoteAddress, this.remotePort);
                }
            }
            catch (SocketException) { }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            finally
            {
                this.listener.Stop();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                Stop();
        }
    }
}
