using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates an async reader/writer lock.
    /// </summary>
    public class AsyncReaderWriterLock
    {
        // >=1 = # of readers
        // 0   = free
        // -1  = 1 writer
        int _count;
        List<Waiter> _waitingReaders = new List<Waiter>();
        readonly Queue<Waiter> _waitingWriters = new Queue<Waiter>();

        /// <summary>
        /// Opens the read lock.
        /// </summary>
        /// <returns>A task that completes when the read lock is open. The IDisposable must be disposed to release the lock.</returns>
        public Task<IDisposable> OpenReader()
        {
            var reader = new Waiter(this, false);
            bool entered;
            lock (_waitingWriters)
            {
                if (_count == 0 || (_count > 0 && _waitingWriters.Count == 0))
                {
                    ++_count;
                    entered = true;
                }
                else
                {
                    _waitingReaders.Add(reader);
                    entered = false;
                }
            }
            if (entered)
            {
                reader.CompletionSource.SetResult(reader.Disposable);
            }
            return reader.CompletionSource.Task;
        }

        /// <summary>
        /// Opens the write lock.
        /// </summary>
        /// <returns>A task that completes when the write lock is open. The IDisposable must be disposed to release the lock.</returns>
        public Task<IDisposable> OpenWriter()
        {
            var writer = new Waiter(this, true);
            bool entered;
            lock (_waitingWriters)
            {
                if (_count == 0)
                {
                    --_count;
                    entered = true;
                }
                else
                {
                    _waitingWriters.Enqueue(writer);
                    entered = false;
                }
            }
            if (entered)
            {
                writer.Release();
            }
            return writer.CompletionSource.Task;
        }

        void CloseReader()
        {
            var waiter = default(Waiter);
            lock (_waitingWriters)
            {
                if (_count <= 0) throw new InvalidOperationException("Too many attempts have been made to release the read lock.");
                if (--_count == 0 && _waitingWriters.Count > 0)
                {
                    --_count;
                    waiter = _waitingWriters.Dequeue();
                }
            }
            if (waiter.CompletionSource != null) waiter.Release();
        }

        void CloseWriter()
        {
            var writer = default(Waiter);
            List<Waiter> readers = null;
            lock (_waitingWriters)
            {
                if (_count >= 0) throw new InvalidOperationException("Too many attempts have been made to release the write lock.");
                if (_count < -1) throw new InvalidOperationException(GetType().Name + " is in an invalid state: count = " + _count);
                if (_waitingReaders.Count > 0)
                {
                    readers = _waitingReaders;
                    _waitingReaders = new List<Waiter>();
                    _count = readers.Count;
                }
                else if (_waitingWriters.Count > 0)
                {
                    writer = _waitingWriters.Dequeue();
                }
                else
                {
                    _count = 0;
                }
            }
            if (readers != null)
            {
                foreach (var reader in readers)
                {
                    reader.Release();
                }
            }
            else if (writer.CompletionSource != null)
            {
                writer.Release();
            }
        }

        private struct Waiter
        {
            public Waiter(AsyncReaderWriterLock owner, bool write)
            {
                CompletionSource = new TaskCompletionSource<IDisposable>();
                Disposable = new ReleaseDisposable(owner, write);
            }
            public readonly TaskCompletionSource<IDisposable> CompletionSource;
            public readonly ReleaseDisposable Disposable;
            public void Release()
            {
                CompletionSource.SetResult(Disposable);
            }
        }

        private class ReleaseDisposable : IDisposable
        {
            AsyncReaderWriterLock _owner;
            readonly bool _write;
            public ReleaseDisposable(AsyncReaderWriterLock owner, bool write)
            {
                _owner = owner;
                _write = write;
            }

            public void Dispose()
            {
                if (_owner != null)
                {
                    if (_write)
                    {
                        _owner.CloseWriter();
                    }
                    else
                    {
                        _owner.CloseReader();
                    }
                    _owner = null;
                }
            }
        }
    }
}
