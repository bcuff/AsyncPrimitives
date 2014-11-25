using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    public class SimpleSynchronizationContext : SynchronizationContext
    {
        private struct Work
        {
             public SendOrPostCallback Callback;
             public object State;
             public ManualResetEventSlim Handle;
        }

        int _waiterCount;
        bool _executing;
        TaskCompletionSource<int> _stopSource;
        readonly Queue<Work> _workQueue = new Queue<Work>();

        public override void Post(SendOrPostCallback d, object state)
        {
            QueueWork(new Work
            {
                Callback = d,
                State = state,
            });
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            using (var handle = new ManualResetEventSlim(false))
            {
                QueueWork(new Work
                {
                    Callback = d,
                    State = state,
                    Handle = handle,
                });
                handle.Wait();
            }
        }

        public void Execute()
        {
            lock (_workQueue)
            {
                if (_executing) throw new InvalidOperationException("Already executing!");
                _executing = true;
            }
            try
            {
                while (true)
                {
                    Work work;
                    lock (_workQueue)
                    {
                        if (_stopSource != null) break;
                        while (_workQueue.Count < 0)
                        {
                            ++_waiterCount;
                            Monitor.Wait(_workQueue);
                            --_waiterCount;
                            if (_stopSource != null) break;
                        }
                        work = _workQueue.Dequeue();
                    }
                    try
                    {
                        work.Callback(work.State);
                    }
                    finally
                    {
                        if (work.Handle != null) work.Handle.Set();
                    }
                }
            }
            finally
            {
                TaskCompletionSource<int> stopSource;
                lock (_workQueue)
                {
                    _executing = false;
                    stopSource = _stopSource;
                    _stopSource = null;
                }
                if (stopSource != null)
                {
                    stopSource.SetResult(1);
                }
            }
        }

        public Task Stop()
        {
            TaskCompletionSource<int> stopSource;
            lock (_workQueue)
            {
                if (_stopSource != null)
                {
                    stopSource = _stopSource;
                }
                else
                {
                    stopSource = _stopSource = new TaskCompletionSource<int>();
                    if (_waiterCount > 0) Monitor.Pulse(_workQueue);
                }
            }
            return stopSource.Task;
        }

        void QueueWork(Work work)
        {
            lock (_workQueue)
            {
                if (_stopSource != null) throw new InvalidOperationException("Can't Post or Send new work. Context has been stopped.");
                _workQueue.Enqueue(work);
                if (_workQueue.Count > 1 && _waiterCount > 0)
                {
                    Monitor.Pulse(_workQueue);
                }
            }
        }
    }
}
