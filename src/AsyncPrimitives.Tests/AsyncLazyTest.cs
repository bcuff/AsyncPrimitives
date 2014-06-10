using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsyncPrimitives.Tests
{
    [TestClass]
    public class AsyncLazyTest
    {
        [TestMethod]
        public async Task TestGetAwaiter()
        {
            var target = new AsyncLazy<int>(async () => { await Task.Delay(1); return 1; });
            Assert.AreEqual(1, await target);
        }
    }
}
