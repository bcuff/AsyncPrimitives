using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncPrimitives
{
    public class HeavyHitterTracker<T> : CountMinSketch<T>
    {
        readonly int _maxItems;
        readonly Dictionary<T, LinkedListNode<HeavyHitter<T>>> _heavyHittersByValue;
        readonly LinkedList<HeavyHitter<T>> _heavyHittersByCount; 

        public HeavyHitterTracker(int width, int depth, int maxItems, Func<T, int, int> hashFunc) : base(width, depth, hashFunc)
        {
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
