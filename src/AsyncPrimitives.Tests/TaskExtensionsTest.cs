using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class TaskExtensionsTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor1Validation()
        {
            TaskExtensions.ToAsyncResult(null, ar => { }, "foo");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructor2Validation()
        {
            TaskExtensions.ToAsyncResult<int>(null, ar => { }, "foo");
        }

        [TestMethod]
        public void TestTypedSynchronousEnd()
        {
            var source = new TaskCompletionSource<int>();
            var ar = source.Task.ToAsyncResult(null, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetResult(123);
            Assert.AreEqual(123, ar.End());
        }

        [TestMethod]
        public void TestUntypedSynchronousEnd()
        {
            var source = new TaskCompletionSource<int>();
            Task task = source.Task;
            var ar = task.ToAsyncResult(null, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetResult(1);
            ar.End();
        }

        [TestMethod]
        public void TestTypedCallback()
        {
            var source = new TaskCompletionSource<int>();
            var handle = new ManualResetEventSlim(false);
            int result = 0;
            var ar = source.Task.ToAsyncResult(asyncResult =>
            {
                result = ((TaskAsyncResult<int>)asyncResult).End();
                handle.Set();
            }, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetResult(123);
            handle.Wait();
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void TestUntypedCallback()
        {
            var source = new TaskCompletionSource<int>();
            Task task = source.Task;
            var handle = new ManualResetEventSlim(false);
            var ar = task.ToAsyncResult(asyncResult =>
            {
                ((TaskAsyncResult)asyncResult).End();
                handle.Set();
            }, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetResult(123);
            handle.Wait();
        }

        [TestMethod]
        public void TestTypedErrorCallback()
        {
            var source = new TaskCompletionSource<int>();
            var handle = new ManualResetEventSlim(false);
            Exception result = null;
            var ar = source.Task.ToAsyncResult(asyncResult =>
            {
                try
                {
                    ((TaskAsyncResult<int>)asyncResult).End();
                }
                catch (Exception ex)
                {
                    result = ex;
                }
                handle.Set();
            }, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetException(new Exception("bar"));
            handle.Wait();
            Assert.AreEqual("bar", result.InnerException.Message);
        }

        [TestMethod]
        public void TestUntypedErrorCallback()
        {
            var source = new TaskCompletionSource<int>();
            Task task = source.Task;
            var handle = new ManualResetEventSlim(false);
            Exception result = null;
            var ar = task.ToAsyncResult(asyncResult =>
            {
                try
                {
                    ((TaskAsyncResult)asyncResult).End();
                }
                catch (Exception ex)
                {
                    result = ex;
                }
                handle.Set();
            }, "foo");
            Assert.AreEqual("foo", ar.AsyncState);
            Assert.AreEqual(false, ar.IsCompleted);
            source.SetException(new Exception("bar"));
            handle.Wait();
            Assert.AreEqual("bar", result.InnerException.Message);
        }
    }
}
