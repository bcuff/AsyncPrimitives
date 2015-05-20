using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    public abstract class TaskAsyncResult : IAsyncResult
    {
        internal TaskAsyncResult() { }

        protected abstract Task Task { get; }

        public object AsyncState
        {
            get { return Task.AsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return ((IAsyncResult)Task).AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return ((IAsyncResult)Task).CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return Task.IsCompleted; }
        }

        public void End()
        {
            Task.Wait();
        }
    }

    public sealed class TaskAsyncResult<T> : TaskAsyncResult
    {
        readonly Task<T> _task;

        internal TaskAsyncResult(Task<T> task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<T>(state);
            _task = tcs.Task;
            task.ContinueWith(t =>
            {
                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(t.Result);
                if (callback != null) callback(this);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        public new T End()
        {
            return ((Task<T>)Task).Result;
        }

        protected override Task Task
        {
            get { return _task; }
        }
    }
}
