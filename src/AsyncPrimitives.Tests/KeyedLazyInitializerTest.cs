using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    /// <summary>
    /// Tests the <see cref="KeyedLazyInitializer{TKey,TValue}"/> class.
    /// </summary>
    [TestClass]
    public class KeyedLazyInitializerTest
    {
        /// <summary>
        /// Tests <see cref="KeyedLazyInitializer{TKey,TValue}(Func{TKey,TValue})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor1()
        {
            new KeyedLazyInitializer<int, int>(null);
        }

        /// <summary>
        /// Tests <see cref="KeyedLazyInitializer{TKey,TValue}(Func{TKey,TValue})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor2()
        {
            new KeyedLazyInitializer<int, int>(null, null);
        }

        /// <summary>
        /// Tests <see cref="KeyedLazyInitializer{TKey,TValue}.Item"/>.
        /// </summary>
        [TestMethod]
        public void TestItem()
        {
            int callCount = 0;

            var target = new KeyedLazyInitializer<int, string>(i =>
            {
                callCount++;
                return i.ToString();
            });

            for (int i = 0; i < 1000; ++i)
            {
                Assert.AreEqual(i.ToString(), target[i]);
            }

            Assert.AreEqual(1000, callCount);
            Assert.AreEqual(1000, target.Count);


            for (int i = 0; i < 1000; ++i)
            {
                Assert.AreEqual(i.ToString(), target[i]);
            }

            Assert.AreEqual(1000, callCount);
            Assert.AreEqual(1000, target.Count);
        }

        /// <summary>
        /// Tests <see cref="KeyedLazyInitializer{TKey,TValue}.Remove"/>.
        /// </summary>
        [TestMethod]
        public void TestRemove()
        {
            int callCount = 0;

            var target = new KeyedLazyInitializer<int, string>(i =>
            {
                callCount++;
                return i.ToString();
            });

            Assert.AreEqual("123", target[123]);

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(1, target.Count);

            Assert.AreEqual(false, target.Remove(456));
            Assert.AreEqual(true, target.Remove(123));

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(0, target.Count);


            Assert.AreEqual("123", target[123]);

            Assert.AreEqual(2, callCount);
            Assert.AreEqual(1, target.Count);
        }

        /// <summary>
        /// Tests <see cref="KeyedLazyInitializer{TKey,TValue}.Clear"/>.
        /// </summary>
        [TestMethod]
        public void TestClear()
        {
            int callCount = 0;

            var target = new KeyedLazyInitializer<int, string>(i =>
            {
                callCount++;
                return i.ToString();
            });

            var foo = target[1];
            foo = target[2];

            Assert.AreEqual(2, target.Count);

            target.Clear();

            Assert.AreEqual(0, target.Count);

            foo = target[1];
            foo = target[2];

            Assert.AreEqual(4, callCount);
            Assert.AreEqual(2, target.Count);
        }
    }
}
