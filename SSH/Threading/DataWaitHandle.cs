using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SSH.Threading
{
    public class DataWaitHandle<T> : EventWaitHandle where T : class
    {
        public bool IsSet { get; private set; }
        public T Result { get; set; }
        public Exception Exception { get; set; }

        public DataWaitHandle(bool initialState, EventResetMode mode)
            : base(initialState, mode)
        {
            IsSet = initialState;
        }

        public new bool Reset()
        {
            IsSet = false;
            return base.Reset();
        }

        public new bool Set()
        {
            this.IsSet = true;
            this.Result = null;
            return base.Set();
        }

        public bool Set(T o)
        {
            this.IsSet = true;
            this.Result = o;
            return base.Set();
        }

        public bool Set(Exception ex)
        {
            this.IsSet = true;
            this.Exception = ex;
            return base.Set();
        }

        public override bool WaitOne()
        {
            return this.WaitOne(-1);
        }

        public override bool WaitOne(int millisecondsTimeout)
        {
            return this.WaitOne(millisecondsTimeout, false);
        }

        public override bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            var result = base.WaitOne(millisecondsTimeout, exitContext);
            if (this.Exception != null)
                throw this.Exception;
            return result;
        }

        public override bool WaitOne(TimeSpan timeout)
        {
            return this.WaitOne((int)timeout.TotalMilliseconds);
        }

        public override bool WaitOne(TimeSpan timeout, bool exitContext)
        {
            return this.WaitOne((int)timeout.TotalMilliseconds, exitContext);
        }
    }
}
