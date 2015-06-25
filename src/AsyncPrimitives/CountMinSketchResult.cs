using System;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates the result of a CountMinSketch data point.
    /// </summary>
    public struct CountMinSketchResult
    {
        private readonly long _count;
        private readonly long _totalCount;

        internal CountMinSketchResult(long count, long totalCount)
        {
            _count = count;
            _totalCount = totalCount;
        }

        /// <summary>
        /// The estimated number of times the values was incremented.
        /// </summary>
        public long Count
        {
            get { return _count; }
        }

        /// <summary>
        /// The total number of times the entire sketch structure was incremented.
        /// </summary>
        public long TotalCount
        {
            get { return _totalCount; }
        }
    }
}
