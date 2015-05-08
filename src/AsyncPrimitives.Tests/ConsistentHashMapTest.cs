using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
	[TestClass]
	public class ConsistentHashMapTest
	{
		[TestMethod]
		public void TestCount()
		{
			var values = new[] { "1", "2", "3" };
			var target = new ConsistentHashMap<int, string>(values, null);
			Assert.AreEqual(values.Length, target.Count);
		}

		[TestMethod]
		public void TestGet1()
		{
			var values = new[] { "1", "2", "3" };
			var target = new ConsistentHashMap<int, string>(values, null);
			CollectionAssert.Contains(values, target.Get(123));
		}

		[TestMethod]
		public void TestGet2()
		{
			var values = new[] { "1", "2", "3" };
			var target = new ConsistentHashMap<int, string>(values, null);

			var expected = target.GetMany(123).Take(values.Length + 1).OrderBy(s => s).ToArray();
			CollectionAssert.AreEqual(values, expected);

			expected = target.GetMany(123).Take(values.Length - 1).ToArray();
			Assert.AreEqual(values.Length - 1, expected.Length);
			foreach (var val in expected)
			{
				CollectionAssert.Contains(values, val);
			}
		}

		[TestMethod]
		public void TestHashCollisions()
		{
			var values = Enumerable.Range(0, 100);

			Action test = () =>
			{
				var target = new ConsistentHashMap<int, int>(values, new ConsistentHashMapSettings<int, int>
				{
					VirtualNodeCountProvider = value => 2,
					ValueHashDataProvider = value => "/" + value,
					HashFunction = value =>
					{
						value = value.Substring(value.IndexOf('/') + 1);
						var intValue = int.Parse(value);
						if (intValue == 51) return 50;
						return (ulong)intValue;
					}
				});

				var nodes = target.GetMany(50).ToArray();

				Assert.AreEqual(50, nodes[0]);
				Assert.AreEqual(51, nodes[1]);

				Assert.AreEqual(values.Count(), target.Count);
				Assert.AreEqual(values.Count(), target.GetMany(0).Count());
			};

			test();

			values = Enumerable.Range(0, 100).Reverse();

			test();

			values = Enumerable.Range(0, 10);

			var settings = new ConsistentHashMapSettings<int, int>
			{
				VirtualNodeCountProvider = value => 2,
				HashFunction = s => 0,
			};
			var map = new ConsistentHashMap<int, int>(values, settings);

			CollectionAssert.AreEqual(values.ToArray(), map.GetMany(1231).ToArray());

			map = new ConsistentHashMap<int, int>(values.Reverse(), settings);

			CollectionAssert.AreEqual(values.ToArray(), map.GetMany(1231).ToArray());

			settings = new ConsistentHashMapSettings<int, int>
			{
				VirtualNodeCountProvider = value => 2,
				HashFunction = s => (ulong)(int.Parse(s) * 2 / values.Count()),
			};
			map = new ConsistentHashMap<int, int>(values, settings);

			CollectionAssert.AreEqual(values.ToArray(), map.GetMany(1231).ToArray());

			map = new ConsistentHashMap<int, int>(values.Reverse(), settings);

			CollectionAssert.AreEqual(values.ToArray(), map.GetMany(1231).ToArray());
		}

		[TestMethod]
		public void TestDistribution()
		{
			const int count = 100;
			const int getsPerNode = 10000;
			const double allowableDeviation = .12d;

			var values = new Dictionary<string, int>(count);
			for (int i = 0; i < count; ++i)
			{
				values["test-node" + i] = 0;
			}

			var stopwatch = Stopwatch.StartNew();
			var target = new ConsistentHashMap<int, string>(values.Keys, new ConsistentHashMapSettings<int,string>());
			stopwatch.Stop();
			Trace.WriteLine(string.Format("Map created in {0:0.00} seconds.", stopwatch.Elapsed.TotalSeconds));

			for (int i = 0; i < (count * getsPerNode); ++i)
			{
				values[target.Get(i)]++;
			}

			var average = values.Values.Average(i => i);
			double? maxDeviation = null;

			foreach (var value in values.Values)
			{
				var deviation = (value / average) - 1.0;
				if (maxDeviation == null)
				{
					maxDeviation = deviation;
				}
				else
				{
					if (Math.Abs(maxDeviation.Value) < Math.Abs(deviation))
					{
						maxDeviation = deviation;
					}
				}
			}
			Trace.WriteLine(string.Format("Count = {0}, MaxDeviation={1}", count, maxDeviation));
			Assert.AreEqual(true, Math.Abs(maxDeviation.Value) < allowableDeviation);
		}
	}
}
