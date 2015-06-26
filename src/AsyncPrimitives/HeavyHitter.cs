using System;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates a heavy hitter in the HeavyHitterTracker class.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public struct HeavyHitter<T>
    {
        readonly T _value;
        readonly long _count;

        internal HeavyHitter(T value, long count)
        {
            _value = value;
            _count = count;
        }

        /// <summary>
        /// The value of the heavy hitter.
        /// </summary>
        public T Value
        {
            get { return _value; }
        }

        /// <summary>
        /// The estimated number of times the value was encountered.
        /// This value may be more than the actual count but will not be less.
        /// </summary>
        public long Count
        {
            get { return _count; }
        }
    }
}
