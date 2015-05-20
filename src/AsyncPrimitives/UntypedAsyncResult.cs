using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    internal class UntypedTaskAsyncResult : TaskAsyncResult
    {
        readonly Task _task;

        internal UntypedTaskAsyncResult(Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<int>(state);
            _task = tcs.Task;
            task.ContinueWith(t =>
            {
                if (t.IsFaulted) tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) tcs.TrySetCanceled();
                else tcs.TrySetResult(1);
                if (callback != null) callback(this);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        protected override Task Task
        {
            get { return _task; }
        }
    }
}
