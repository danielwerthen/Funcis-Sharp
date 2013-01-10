using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Knx.Router;

namespace Knx.Tests.RouterTests
{
    [TestClass]
    public class TestDispose
    {
        [TestMethod]
        public void TestDisposeMethod()
        {
            using (var router = new RouterActor())
            {

            }
        }
    }
}
