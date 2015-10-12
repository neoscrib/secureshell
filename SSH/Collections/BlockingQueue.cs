using System;
using System.Collections.Generic;
using System.Threading;

namespace SSH.Collections
{
    /// <summary>
    /// Represents a first-in, first-out collection of objects. The Dequeue method will block until an item is available to be dequeued.
    /// </summary>
    /// <typeparam name="T">is T</typeparam>
    public class BlockingQueue<T> : Queue<T>, IDisposable
    {
        Semaphore semRead = new Semaphore(0, int.MaxValue);
        object lockObject = new object();
        
        /// <summary>
        /// Adds an object to the end of the BlockingQueue&lt;T&gt;
        /// </summary>
        /// <param name="item">The object to add to the BlockingQueue&lt;T&gt;. The value can be null for reference types.</param>
        public new void Enqueue(T item)
        {
            lock (lockObject)
            {
                base.Enqueue(item);
                semRead.Release();
            }
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the BlockingQueue&lt;T&gt;. If there are no objects, blocks until an object is available.
        /// </summary>
        /// <returns>The object at the beginning of the BlockingQueue&lt;T&gt;.</returns>
        public new T Dequeue()
        {
            semRead.WaitOne();
            return base.Dequeue();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                semRead.Dispose();
        }
    }
}
