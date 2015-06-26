using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates information about the current heavy hitters
    /// tracked by the HeavyHittersTracker class.
    /// </summary>
    /// <typeparam name="T">The type of item tracked.</typeparam>
    public class HeavyHitterTrackerResult<T>
    {
        internal HeavyHitterTrackerResult() { }

        /// <summary>
        /// The top heavy hitters.
        /// </summary>
        public IReadOnlyCollection<HeavyHitter<T>> HeavyHitters { get; internal set; }

        /// <summary>
        /// The total number of times the heavy hitter instance was incremented for all values.
        /// This is useful if you want to get some sort of estimate on the frequency
        /// of an item relative to others.
        /// </summary>
        public long TotalCount { get; internal set; }
    }
}
