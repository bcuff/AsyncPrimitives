using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// An async semaphore implementation.
    /// </summary>
    public class AsyncSemaphore
    {
        readonly Queue<Waiter> _waiters = new Queue<Waiter>();
        int _count;

        /// <summary>
        /// Initializes a new instances of the AsyncSemaphore class.
        /// </summary>
        /// <param name="count">The initial count of the semaphore.</param>
        public AsyncSemaphore(int count)
        {
            _count = count;
        }

        /// <summary>
        /// The current count.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_waiters)
                {
                    return _count;
                }
            }
        }

        /// <summary>
        /// Waits for the semaphore to become available; decrements the count and enters when available.
        /// </summary>
        /// <returns>A task that completes when the enter is complete.</returns>
        public Task Wait()
        {
            return Wait(new Waiter
            {
                Source = new TaskCompletionSource<IDisposable>(),
                Result = null,
            });
        }

        /// <summary>
        /// Waits for the semaphore to become available; decrements the count and enters when available.
        /// </summary>
        /// <returns>
        /// A task with a disposable that must be disposed to release the semaphore when done.
        /// </returns>
        public Task<IDisposable> WaitAndRelease()
        {
            return Wait(new Waiter
            {
                Source = new TaskCompletionSource<IDisposable>(),
                Result = new ReleaseDisposable(this),
            });
        }

        Task<IDisposable> Wait(Waiter waiter)
        {
            bool success;
            lock (_waiters)
            {
                if (_count > 0)
                {
                    --_count;
                    success = true;
                }
                else
                {
                    success = false;
                    _waiters.Enqueue(waiter);
                }
            }

            if (success)
            {
                waiter.Source.SetResult(waiter.Result);
            }

            return waiter.Source.Task;
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        /// <param name="count">The number of times to release.</param>
        public void Release(int count = 1)
        {
            for (; count > 0; --count)
            {
                Waiter? waiter = null;
                lock (_waiters)
                {
                    if (_waiters.Count > 0)
                    {
                        waiter = _waiters.Dequeue();
                    }
                    else
                    {
                        ++_count;
                    }
                }
                if (waiter.HasValue)
                {
                    waiter.Value.Source.SetResult(waiter.Value.Result);
                }
            }
        }

        private struct Waiter
        {
            public TaskCompletionSource<IDisposable> Source;
            public IDisposable Result;
        }

        private class ReleaseDisposable : IDisposable
        {
            AsyncSemaphore _semaphore;

            public ReleaseDisposable(AsyncSemaphore semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_semaphore == null) return;
                _semaphore.Release();
                _semaphore = null;
            }
        }
    }
}
