using System;
using System.ComponentModel;
using System.Threading;

namespace SSH.Threading
{
    public class SemaphoreWorker : BackgroundWorker
    {
        public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
        public new event ProgressChangedEventHandler ProgressChanged;

        Semaphore sem = new Semaphore(0, 1);
        protected override void OnDoWork(DoWorkEventArgs e)
        {
            base.OnDoWork(e);
            sem.Release();
        }

        public void ReportProgress(long position, long length)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, new ProgressChangedEventArgs(position, length));
        }

        public bool WaitOne()
        {
            return sem.WaitOne();
        }

        public bool WaitOne(int millisecondsTimeout)
        {
            return sem.WaitOne(millisecondsTimeout);
        }

        public bool WaitOne(TimeSpan timeout)
        {
            return sem.WaitOne(timeout);
        }

        public bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            return sem.WaitOne(millisecondsTimeout, exitContext);
        }

        public bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            return sem.WaitOne(timeout, exitContext);
        }
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public double Position { get; set; }
        public double Length { get; set; }

        public ProgressChangedEventArgs(double position, double length)
        {
            this.Position = position;
            this.Length = length;
        }

        public double ProgressPercentage
        {
            get
            {
                return Position / Length * 100d;
            }
        }
    }
}
