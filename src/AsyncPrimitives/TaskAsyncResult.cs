using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// An IAsyncResult implementation that you can use to implement APM with tasks.
    /// </summary>
    public abstract class TaskAsyncResult : IAsyncResult
    {
        internal TaskAsyncResult() { }

        internal abstract Task Task { get; }

        /// <inheritdoc />
        public object AsyncState
        {
            get { return Task.AsyncState; }
        }

        /// <inheritdoc />
        public WaitHandle AsyncWaitHandle
        {
            get { return ((IAsyncResult)Task).AsyncWaitHandle; }
        }

        /// <inheritdoc />
        public bool CompletedSynchronously
        {
            get { return ((IAsyncResult)Task).CompletedSynchronously; }
        }

        /// <inheritdoc />
        public bool IsCompleted
        {
            get { return Task.IsCompleted; }
        }

        /// <summary>
        /// Ends the operation. Call this in your own End method when implementing APM.
        /// </summary>
        public void End()
        {
            Task.Wait();
        }
    }

    /// <summary>
    /// An IAsyncResult implementation that you can use to implement APM with tasks.
    /// </summary>
    /// <typeparam name="T">The type to return in the End method.</typeparam>
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

        /// <summary>
        /// Ends the operation and returns the result. Call this in your own End method when implementing APM.
        /// </summary>
        /// <returns>The result.</returns>
        public new T End()
        {
            return ((Task<T>)Task).Result;
        }

        internal override Task Task
        {
            get { return _task; }
        }
    }
}
