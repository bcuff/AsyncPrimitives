using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class CountMinSketchTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructorWidthValidation()
        {
            new CountMinSketch<int>(0, 1, (a, b) => a ^ b);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestConstructorDepthValidation()
        {
            new CountMinSketch<int>(1, 0, (a, b) => a ^ b);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorHashFuncValidation()
        {
            new CountMinSketch<int>(1, 1, null);
        }

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
            var sketch = new CountMinSketch<string>(100, 8, (val, index) => (val + index).GetHashCode());
            foreach (var item in input)
            {
                var before = sketch.GetEstimatedCount(item.value);
                var after = sketch.Add(item.value, item.count);
                Assert.AreEqual(before.Count + item.count, after.Count);
                Assert.AreEqual(before.TotalCount + item.count, after.TotalCount);
            }
            var allowedError = input.Sum(item => item.count) / input.Length + 1;
            Trace.WriteLine("Allowed Error: " + allowedError);
            var outputQ = from item in input
                          let result = sketch.GetEstimatedCount(item.value)
                          orderby result.Count descending
                          select new { item, result, };
            foreach (var row in outputQ)
            {
                var item = row.item;
                var result = row.result;
                Trace.WriteLine(string.Format("{0}: real={1,3} actual={2,3}", item.value, item.count, result.Count));
            }
            var q = from item in input
                    let result = sketch.GetEstimatedCount(item.value)
                    orderby result.Count descending
                    select item;
            var heavyHitters = q.Take(3).ToArray();
            Assert.AreEqual("heavy3", heavyHitters[0].value);
            Assert.AreEqual("heavy2", heavyHitters[1].value);
            Assert.AreEqual("heavy1", heavyHitters[2].value);
        }
    }
}
