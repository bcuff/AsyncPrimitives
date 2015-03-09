using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class StreamExtensionsTest
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd1ThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEnd(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd2ThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEnd(null, 123);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd3ThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEnd(null, new byte[123]);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd1AsyncThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEndAsync(null).Wait();
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd2AsyncThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEndAsync(null, 123).Wait();
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEnd3AsyncThrowsArgumentNullException()
        {
            StreamExtensions.ReadToEndAsync(null, new byte[123]).Wait();
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestReadToEndBufferSizeValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEnd(0);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestReadToEndBufferSizeAsyncValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEndAsync(0).Wait();
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEndNullBufferValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEnd((byte[])null);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void TestReadToEndAsyncNullBufferValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEndAsync((byte[])null).Wait();
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestReadToEndZeroLengthBufferValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEnd(new byte[0]);
            }
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void TestReadToEndAsyncZeroLengthBufferValidation()
        {
            using (var stream = new MemoryStream())
            {
                stream.ReadToEndAsync(new byte[0]).Wait();
            }
        }

        [TestMethod]
        public void TestReadToEndWithSmallBuffer()
        {
            var random = new Random();
            var bytes = new byte[1000];
            random.NextBytes(bytes);

            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                var result = stream.ReadToEnd(new byte[7]);
                CollectionAssert.AreEqual(bytes, result);
            }
        }

        [TestMethod]
        public void TestReadToEndAsyncWithSmallBuffer()
        {
            var random = new Random();
            var bytes = new byte[1000];
            random.NextBytes(bytes);

            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                var result = stream.ReadToEndAsync(new byte[7]).Result;
                CollectionAssert.AreEqual(bytes, result);
            }
        }
    }
}
