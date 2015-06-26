using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class HeavyHitterTrackerTest
    {
        [TestMethod]
        public void TestFunctionality()
        {
            var random = new Random(123);
            var input = Enumerable.Range(0, 1000).Select(i => new
            {
                value = "foo" + i + "bar",
                count = random.Next(1, 100),
            })
            .Concat(new[]
            {
                new 
                {
                    value = "heavy1",
                    count = 10000,
                },
                new 
                {
                    value = "heavy2",
                    count = 50000,
                },
                new 
                {
                    value = "heavy3",
                    count = 90000,
                },
            })
            .ToArray();
            var sketch = new HeavyHitterTracker<string>(100, 8, 10, (val, index) => (val + index).GetHashCode());
            foreach (var item in input)
            {
                var before = sketch.GetEstimatedCount(item.value);
                var after = sketch.Add(item.value, item.count);
                Assert.AreEqual(before.Count + item.count, after.Count);
                Assert.AreEqual(before.TotalCount + item.count, after.TotalCount);
            }
            var q = from item in input
                    let result = sketch.GetEstimatedCount(item.value)
                    orderby result.Count descending
                    select new { count = result.Count, item.value };
            var heavyHitters = q.Take(10).ToArray();
            Assert.AreEqual("heavy3", heavyHitters[0].value);
            Assert.AreEqual("heavy2", heavyHitters[1].value);
            Assert.AreEqual("heavy1", heavyHitters[2].value);
            var trackedHeavyHitter = sketch.GetHeavyHitters();
            foreach (var heavyHitter in trackedHeavyHitter.HeavyHitters)
            {
                Trace.WriteLine(string.Format("{0}: actual={1,3}", heavyHitter.Value, heavyHitter.Count));
            }
            Assert.AreEqual(sketch.GetEstimatedCount("heavy3").TotalCount, trackedHeavyHitter.TotalCount);
            Assert.AreEqual(10, trackedHeavyHitter.HeavyHitters.Count);
            var pairs = heavyHitters.Zip(trackedHeavyHitter.HeavyHitters, (expected, actual) => new { expected, actual });
            foreach (var pair in pairs)
            {
                Assert.AreEqual(pair.expected.count, pair.actual.Count);
                Assert.AreEqual(pair.expected.value, pair.actual.Value);
            }
        }
    }
}
