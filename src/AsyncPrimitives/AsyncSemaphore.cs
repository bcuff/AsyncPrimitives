using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    public class AsyncSemaphore
    {
        readonly Queue<Waiter> _waiters = new Queue<Waiter>();
        int _count;

        public AsyncSemaphore(int count)
        {
            _count = count;
        }

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

        public Task Wait()
        {
            return Wait(new Waiter
            {
                Source = new TaskCompletionSource<IDisposable>(),
                Result = null,
            });
        }

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
