using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SSH.Processor;

namespace SSH
{
    public class DynamicSocks : IDisposable
    {
        private TcpListener listener;
        private Thread thread;
        private Session session;
        private IPEndPoint local;

        public DynamicSocks(Session session, IPEndPoint local)
        {
            session.Disposables.Add(this);
            this.session = session;
            this.listener = new TcpListener(this.local = local);
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
                    new SocksLocalForward(this.session, client);
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
            Stop();
        }
    }
}
