using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncPrimitives
{
    /// <summary>
    /// A class for tracking the most frequently encountered items.
    /// </summary>
    /// <typeparam name="T">The type of item.</typeparam>
    public class HeavyHitterTracker<T> : CountMinSketch<T>
    {
        readonly int _maxItems;
        readonly Dictionary<T, LinkedListNode<HeavyHitter<T>>> _heavyHittersByValue;
        readonly LinkedList<HeavyHitter<T>> _heavyHittersByCount; 

        /// <summary>
        /// Initializes a new instance of the HeavyHitterTracker class.
        /// </summary>
        /// <param name="width">The width of the sketch.</param>
        /// <param name="depth">The depth of the sketch.</param>
        /// <param name="maxItems">The maximum number of items to track.</param>
        /// <param name="hashFunc">A hash function that produces a hash over the value and index arguments.</param>
        public HeavyHitterTracker(int width, int depth, int maxItems, Func<T, int, int> hashFunc) : base(width, depth, hashFunc)
        {
            if (maxItems < 1) throw new ArgumentOutOfRangeException("maxItems");
            _maxItems = maxItems;
            _heavyHittersByValue = new Dictionary<T, LinkedListNode<HeavyHitter<T>>>();
            _heavyHittersByCount = new LinkedList<HeavyHitter<T>>();
        }

        internal override void OnAddSynchronized(T value, long amount, long count, long totalCount)
        {
            LinkedListNode<HeavyHitter<T>> node;
            if (_heavyHittersByValue.TryGetValue(value, out node))
            {
                node.Value = new HeavyHitter<T>(value, count);
                SiftUp(node);
            }
            else
            {
                node = _heavyHittersByCount.AddLast(new HeavyHitter<T>(value, count));
                _heavyHittersByValue.Add(value, node);
                SiftUp(node);
                if (_heavyHittersByCount.Count > _maxItems)
                {
                    node = _heavyHittersByCount.Last;
                    _heavyHittersByCount.RemoveLast();
                    _heavyHittersByValue.Remove(node.Value.Value);
                }
            }
        }

        void SiftUp(LinkedListNode<HeavyHitter<T>> node)
        {
            for (var prev = node.Previous; prev != null && prev.Value.Count < node.Value.Count; prev = node.Previous)
            {
                _heavyHittersByCount.Remove(prev);
                _heavyHittersByCount.AddAfter(node, prev);
            }
        }

        /// <summary>
        /// Gets the specified number of heavy hitters tracked by this class.
        /// </summary>
        /// <param name="count">The maximum number of heavy hitters to get.</param>
        /// <returns>A summary of heavy hitters tracked by this class.</returns>
        public HeavyHitterTrackerResult<T> GetHeavyHitters(int? count = null)
        {
            long totalCount;
            var result = new List<HeavyHitter<T>>(count ?? (_maxItems > 100 ? 100 : _maxItems));
            lock (SyncRoot)
            {
                result.AddRange((count.HasValue ? _heavyHittersByCount.Take(count.Value) : _heavyHittersByCount));
                totalCount = TotalCount;
            }
            return new HeavyHitterTrackerResult<T>
            {
                HeavyHitters = result,
                TotalCount = totalCount,
            };
        }
    }
}
