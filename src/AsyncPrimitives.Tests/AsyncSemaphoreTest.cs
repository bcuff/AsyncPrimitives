using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class AsyncSemaphoreTest
    {
        [TestMethod]
        public void TestWait()
        {
            var semaphore = new AsyncSemaphore(2);

            Assert.AreEqual(2, semaphore.Count);

            var result = semaphore.Wait();
            Assert.AreEqual(true, result.IsCompleted);
            Assert.AreEqual(1, semaphore.Count);

            result = semaphore.Wait();
            Assert.AreEqual(true, result.IsCompleted);
            Assert.AreEqual(0, semaphore.Count);

            result = semaphore.Wait();
            Assert.AreEqual(false, result.IsCompleted);
            Assert.AreEqual(0, semaphore.Count);

            semaphore.Signal();
            result.Wait();
            Assert.AreEqual(true, result.IsCompleted);
            Assert.AreEqual(0, semaphore.Count);

            semaphore.Signal();
            Assert.AreEqual(1, semaphore.Count);

            semaphore.Signal();
            Assert.AreEqual(2, semaphore.Count);

            semaphore.Signal();
            Assert.AreEqual(3, semaphore.Count);
        }

        [TestMethod]
        public void TestWaitAndSignal()
        {
            var semaphore = new AsyncSemaphore(0);

            var task = semaphore.WaitAndSignal();
            Assert.AreEqual(false, task.IsCompleted);
            Assert.AreEqual(0, semaphore.Count);

            semaphore.Signal(3);

            var disposable = task.Result;
            Assert.AreEqual(2, semaphore.Count);
            disposable.Dispose();
            Assert.AreEqual(3, semaphore.Count);
        }
    }
}
