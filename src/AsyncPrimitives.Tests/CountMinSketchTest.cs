﻿using System;
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
            var input = Enumerable.Range(0, 10000).Select(i => new
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
            var sketch = new CountMinSketch<string>(1000, 12, (val, index) => (val + index).GetHashCode());
            foreach (var item in input)
            {
                var before = sketch.GetEstimatedCount(item.value);
                var after = sketch.Add(item.value, item.count);
                Assert.AreEqual(before.MinCount + item.count, after.MinCount);
                Assert.AreEqual(before.TotalCount + item.count, after.TotalCount);
            }
            var allowedError = input.Sum(item => item.count) / input.Length + 1;
            Trace.WriteLine("Allowed Error: " + allowedError);
            var outputQ = from item in input
                          let result = sketch.GetEstimatedCount(item.value)
                          orderby result.Freqency descending
                          select new { item, result, };
            foreach (var row in outputQ)
            {
                var item = row.item;
                var result = row.result;
                Trace.WriteLine(string.Format("{0}: min={1,3} mean={2,3} actual={3,3} frequency={4:##.0000}", item.value, result.MinCount, result.MeanCount, item.count, result.Freqency));
            }
            var q = from item in input
                    let result = sketch.GetEstimatedCount(item.value)
                    orderby result.MinCount descending
                    select item;
            var heavyHitters = q.Take(3).ToArray();
            Assert.AreEqual("heavy3", heavyHitters[0].value);
            Assert.AreEqual("heavy2", heavyHitters[1].value);
            Assert.AreEqual("heavy1", heavyHitters[2].value);
        }
    }
}
