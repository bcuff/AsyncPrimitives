using System;
using System.Linq;

namespace AsyncPrimitives
{
    public struct HeavyHitter<T>
    {
        readonly T _value;
        readonly long _count;

        internal HeavyHitter(T value, long count)
        {
            _value = value;
            _count = count;
        }

        public T Value
        {
            get { return _value; }
        }

        public long Count
        {
            get { return _count; }
        }
    }
}
