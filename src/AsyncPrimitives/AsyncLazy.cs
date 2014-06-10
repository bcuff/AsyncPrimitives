using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AsyncPrimitives
{
    /// <summary>
    /// Provides support for async lazy initialization.
    /// </summary>
    /// <typeparam name="T">The type to lazily create.</typeparam>
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
        /// </summary>
        /// <param name="taskFactory">The delegate that is invoked to produce the lazily initialized <see cref="Task{T}"/> value when it is needed.</param>
        public AsyncLazy(Func<Task<T>> taskFactory)
            : base(taskFactory, true)
        {
        }

        /// <summary>
        /// Gets an awaiter used to await this <see cref="AsyncLazy{T}"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TaskAwaiter<T> GetAwaiter()
        {
            return Value.GetAwaiter();
        }
    }
}
