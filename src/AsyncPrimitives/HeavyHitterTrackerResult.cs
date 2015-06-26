using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncPrimitives
{
    public class HeavyHitterTrackerResult<T>
    {
        internal HeavyHitterTrackerResult() { }

        public IReadOnlyCollection<HeavyHitter<T>> HeavyHitters { get; internal set; }

        public long TotalCount { get; internal set; }
    }
}
