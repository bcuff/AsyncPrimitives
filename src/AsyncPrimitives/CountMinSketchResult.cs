using System;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// Encapsulates the result of a CountMinSketch data point.
    /// </summary>
    public struct CountMinSketchResult
    {
        private readonly long _minCount;
        private readonly long _meanCount;
        private readonly long _totalCount;

        internal CountMinSketchResult(long minCount, long meanCount, long totalCount)
        {
            _minCount = minCount;
            _meanCount = meanCount;
            _totalCount = totalCount;
        }

        /// <summary>
        /// The estimated frequency of the value. This can be negative due to error.
        /// </summary>
        public double Freqency
        {
            get { return (double)MeanCount / TotalCount; }
        }

        /// <summary>
        /// The estimated number of times the values was incremented using
        /// the min count method. This is not noise adjusted and will
        /// always return a count at least as big as the actual count.
        /// </summary>
        public long MinCount
        {
            get { return _minCount; }
        }

        /// <summary>
        /// The estimated number of times the values was incremented using
        /// the mean min count method. This estimate attempts to improve
        /// accuracy by adjusting for noise. This is probably more accurate
        /// than MinCount however it can return less than the actual count.
        /// Note that this also can be negative due to error.
        /// </summary>
        public long MeanCount
        {
            get { return _meanCount; }
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
