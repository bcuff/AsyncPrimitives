using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AsyncPrimitives
{
    public class ConsistentHashMap<TKey, TValue>
    {
        private static readonly int _nodePrefixPadding = int.MaxValue.ToString().Length;

        private readonly ConsistentHashMapSettings<TKey, TValue> _settings;
        private readonly Node[] _hashCircle;
        private readonly TValue[] _values;

        public ConsistentHashMap(IEnumerable<TValue> values)
            : this(values, null)
        {
        }

        public ConsistentHashMap(IEnumerable<TValue> values, ConsistentHashMapSettings<TKey, TValue> settings)
        {
            if (values == null) throw new ArgumentNullException("values");

            _settings = settings ?? new ConsistentHashMapSettings<TKey, TValue>();

            var hashCircle = new List<Node>();
            var valueList = new List<TValue>();

            foreach (var value in values)
            {
                int virtualNodeCount = _settings.VirtualNodeCountProvider(value);
                string hashData = _settings.ValueHashDataProvider(value);
                valueList.Add(value);
                int valueIndex = valueList.Count - 1;
                for (int i = 0; i < virtualNodeCount; ++i)
                {
                    string prefix = i.ToString().PadLeft(_nodePrefixPadding, '0');
                    ulong hash = _settings.HashFunction(prefix + hashData);
                    hashCircle.Add(new Node(hash, valueIndex));
                }
            }
            Count = valueList.Count;

            if (Count == 0)
            {
                throw new ArgumentException("values may not be empty.", "values");
            }

            hashCircle.Sort((a, b) =>
            {
                var result = a.Hash.CompareTo(b.Hash);
                if (result != 0) return result;

                var aName = _settings.ValueHashDataProvider(valueList[a.ValueIndex]);
                var bName = _settings.ValueHashDataProvider(valueList[b.ValueIndex]);

                result = StringComparer.OrdinalIgnoreCase.Compare(aName, bName);
                if (result != 0) return result;

                return StringComparer.Ordinal.Compare(aName, bName);
            });
            _hashCircle = hashCircle.ToArray();
            _values = valueList.ToArray();

            AssertHashCircleInOrder();
        }

        [Conditional("DEBUG")]
        private void AssertHashCircleInOrder()
        {
            for (int i = 1; i < _hashCircle.Length; ++i)
            {
                var a = _hashCircle[i - 1];
                var b = _hashCircle[i];

                if (a.Hash < b.Hash) continue;

                if (a.Hash == b.Hash)
                {
                    var valueA = _settings.ValueHashDataProvider(_values[a.ValueIndex]);
                    var valueB = _settings.ValueHashDataProvider(_values[b.ValueIndex]);

                    if (StringComparer.Ordinal.Compare(valueA, valueB) <= 0)
                    {
                        continue;
                    }
                }

                throw new ApplicationException(string.Format("Out of order elements found at index {0}.", i));
            }
        }

        private int GetStartIndex(TKey key)
        {
            var hash = _settings.HashFunction(_settings.KeyHashDataProvider(key));
            var index = Array.BinarySearch(_hashCircle, new Node(hash, 0), NodeComparer.Instance);
            if (index < 0)
            {
                index = ~index;
                return index >= _hashCircle.Length ? 0 : index;
            }
            // if we hit the element, we want to make sure
            // there could be more than one element with
            // the same hash value and we want to make sure
            // that we select the index of the first
            // element with that hash value.
            for (int i = 0; i < _hashCircle.Length; ++i)
            {
                --index;
                if (index < 0)
                {
                    index = _hashCircle.Length - 1;
                }
                if (_hashCircle[index].Hash != hash)
                {
                    return index + 1;
                }
            }
            // all values in the circle are the same.
            return 0;
        }

        public TValue Get(TKey key)
        {
            var startIndex = GetStartIndex(key);
            return _values[_hashCircle[startIndex].ValueIndex];
        }

        public IEnumerable<TValue> GetMany(TKey key)
        {
            var count = Count;

            var start = GetStartIndex(key);
            HashSet<int> readValueIndexes = null;

            for (int i = 0; i < _hashCircle.Length && count > 0; ++i)
            {
                var valueIndex = _hashCircle[(start + i) % _hashCircle.Length].ValueIndex;

                if (readValueIndexes == null || !readValueIndexes.Contains(valueIndex))
                {
                    --count;
                    yield return _values[valueIndex];
                }
                if (readValueIndexes == null)
                {
                    readValueIndexes = new HashSet<int>();
                }
                readValueIndexes.Add(valueIndex);
            }
        }

        public IEnumerable<TValue> GetAllUnsorted()
        {
            return _values;
        }

        public int Count { get; private set; }

        public int HashCollisionCount { get; private set; }

        private class NodeComparer : IComparer<Node>
        {
            public static readonly NodeComparer Instance = new NodeComparer();

            private NodeComparer() { }

            public int Compare(Node x, Node y)
            {
                if (x.Hash < y.Hash) return -1;
                if (x.Hash > y.Hash) return 1;
                return 0;
            }
        }

        [DebuggerDisplay("Hash={Hash}, ValueIndex={ValueIndex}")]
        private struct Node
        {
            public Node(ulong hash, int valueIndex)
            {
                Hash = hash;
                ValueIndex = valueIndex;
            }

            public readonly ulong Hash;
            public readonly int ValueIndex;
        }
    }

}
