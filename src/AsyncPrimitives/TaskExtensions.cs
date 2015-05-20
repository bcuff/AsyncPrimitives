using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates extension methods that operate on <see cref="Task" /> and <see cref="Task{T}" /> instances.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Converts the specified task into an <see cref="IAsyncResult"/> implementation that may be used in an APM pattern.
        /// </summary>
        /// <typeparam name="T">The type parameter of the task.</typeparam>
        /// <param name="task">The task to convert.</param>
        /// <param name="callback">The APM callback to invoke upon completion.</param>
        /// <param name="state">The IAsyncResult.AsyncState.</param>
        /// <returns>An IAsyncResult implementation that may be used to implement APM.</returns>
        public static TaskAsyncResult<T> ToAsyncResult<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            return new TaskAsyncResult<T>(task, callback, state);
        }

        /// <summary>
        /// Converts the specified task into an <see cref="IAsyncResult"/> implementation that may be used in an APM pattern.
        /// </summary>
        /// <param name="task">The task to convert.</param>
        /// <param name="callback">The APM callback to invoke upon completion.</param>
        /// <param name="state">The IAsyncResult.AsyncState.</param>
        /// <returns>An IAsyncResult implementation that may be used to implement APM.</returns>
        public static TaskAsyncResult ToAsyncResult(this Task task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            return new UntypedTaskAsyncResult(task, callback, state);
        }
    }
}
