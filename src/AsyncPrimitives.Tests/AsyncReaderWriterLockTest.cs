using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class AsyncReaderWriterLockTest
    {
        [TestMethod]
        public void TestReadersBlockWriter()
        {
            var target = new AsyncReaderWriterLock();

            var reader1 = target.OpenReader();
            Assert.IsTrue(reader1.IsCompleted);

            var writer1 = target.OpenWriter();
            Assert.IsFalse(writer1.IsCompleted);

            // writer should be prioritized
            var reader2 = target.OpenReader();
            Assert.IsFalse(reader2.IsCompleted);

            reader1.Result.Dispose();
            writer1.Wait(100);
            Assert.IsTrue(writer1.IsCompleted);

            Assert.IsFalse(reader2.IsCompleted);
            writer1.Result.Dispose();
            reader2.Wait(100);
            Assert.IsTrue(reader2.IsCompleted);
        }

        [TestMethod]
        public void TestMultipleWritersAreAllowed()
        {
            var target = new AsyncReaderWriterLock();

            var readers = new[] { target.OpenReader(), target.OpenReader(), target.OpenReader() };
            Assert.IsTrue(readers.All(t => t.IsCompleted));

            var writer = target.OpenWriter();
            Assert.IsFalse(writer.IsCompleted);
            readers.First().Result.Dispose();
            Assert.IsFalse(writer.IsCompleted);
            foreach (var reader in readers.Skip(1)) reader.Result.Dispose();
            writer.Wait(100);
            Assert.IsTrue(writer.IsCompleted);
        }

        [TestMethod]
        public void TestWritersBlockReaders()
        {
            var target = new AsyncReaderWriterLock();

            var writer = target.OpenWriter();
            Assert.IsTrue(writer.IsCompleted);

            var readers = Enumerable.Range(0, 3).Select(i => target.OpenReader()).ToArray();
            Assert.IsTrue(readers.All(t => !t.IsCompleted));

            writer.Result.Dispose();
            foreach (var reader in readers) reader.Wait(100);
            Assert.IsTrue(readers.All(t => t.IsCompleted));
        }
    }
}
