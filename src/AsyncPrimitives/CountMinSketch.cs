using System;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// An implementation of the count-min-sketch algorithm.
    /// </summary>
    /// <remarks>All methods in this class are thread safe.</remarks>
    /// <typeparam name="T">
    /// The type of value to count.
    /// </typeparam>
    public class CountMinSketch<T>
    {
        readonly object _syncRoot = new object();
        readonly Func<T, int, int> _hashFunc;
        readonly int _width;
        readonly int _depth;
        readonly long[,] _counts;
        long _totalCount;

        /// <summary>
        /// Initializes a new instances of the CountMinSketch class.
        /// </summary>
        /// <param name="width">The width of the sketch.</param>
        /// <param name="depth">The depth of the sketch.</param>
        /// <param name="hashFunc">A hash function that produces a hash over the value and index arguments.</param>
        public CountMinSketch(int width, int depth, Func<T, int, int> hashFunc)
        {
            if (hashFunc == null) throw new ArgumentNullException("hashFunc");
            if (width < 2) throw new ArgumentOutOfRangeException("width");
            if (depth < 1) throw new ArgumentOutOfRangeException("depth");
            _width = width;
            _depth = depth;
            _hashFunc = hashFunc;
            _counts = new long[width, depth];
        }

        /// <summary>
        /// Adds 1 to the count of the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>The estimated count of that value.</returns>
        public CountMinSketchResult Add(T value)
        {
            return Add(value, 1L);
        }

        /// <summary>
        /// Adds the specified amount to the count of the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="amount">The amount by which to increment.</param>
        /// <returns>The estimated count of that value along with the total count.</returns>
        public unsafe CountMinSketchResult Add(T value, long amount)
        {
            if (amount < 0L) throw new ArgumentOutOfRangeException("amount");
            // Since I expect this to get called w/ high frequency from multiple threads...
            // 1. Compute hashes ahead of time to avoid holding the lock for very long.
            // 2. Use stackalloc to avoid allocating memory on every call.
            int* indexes = stackalloc int[_depth];
            long* meanCounts = stackalloc long[_depth];
            for (int i = 0; i < _depth; ++i)
            {
                indexes[i] = (_hashFunc(value, i) & int.MaxValue) % _width;
            }
            long resultCount = 0, totalCount;
            lock (_syncRoot)
            {
                totalCount = _totalCount += amount;
                for (int i = 0; i < _depth; ++i)
                {
                    var count = _counts[indexes[i], i] += amount;
                    resultCount = i == 0 ? count : Math.Min(resultCount, count);
                    var noiseEstimate = (totalCount - count) / (_width - 1);
                    meanCounts[i] = count - noiseEstimate;
                }
            }
            var meanCount = Median(meanCounts, _depth);
            return new CountMinSketchResult(resultCount, meanCount, totalCount);
        }

        private unsafe static long Median(long* val, int n)
        {
            QuickSort(val, n);
            var midpoint = n / 2;
            if (n % 2 == 0)
            {
                return (val[midpoint - 1] + val[midpoint]) / 2;
            }
            return val[midpoint];
        }

        private unsafe static void QuickSort(long *val, int n)
        {
            int i, j;
            if (n < 2) return;
            var p = val[n / 2];
            for (i = 0, j = n - 1;; i++, j--) {
                while (val[i] < p) i++;
                while (p < val[j]) j--;
                if (i >= j) break;
                var t = val[i];
                val[i] = val[j];
                val[j] = t;
            }
            QuickSort(val, i);
            QuickSort(val + i, n - i);
        }

        /// <summary>
        /// Gets the estimated count of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The estimated count of the specified value.</returns>
        public CountMinSketchResult GetEstimatedCount(T value)
        {
            return Add(value, 0L);
        }
    }
}
