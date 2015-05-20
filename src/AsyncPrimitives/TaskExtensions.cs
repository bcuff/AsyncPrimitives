using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    public static class TaskExtensions
    {
        public static TaskAsyncResult<T> ToAsyncResult<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            return new TaskAsyncResult<T>(task, callback, state);
        }

        public static TaskAsyncResult ToAsyncResult(this Task task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            return new UntypedTaskAsyncResult(task, callback, state);
        }
    }
}
