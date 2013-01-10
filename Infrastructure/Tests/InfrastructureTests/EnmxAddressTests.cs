using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Knx.Infrastructure.DataTypes;

namespace Knx.Tests.InfrastructureTests
{
    [TestClass]
    public class EnmxAddressTests
    {
        [TestMethod]
        public void TestEnmxAddress()
        {
            string a1 = "1/4/30";
            EnmxAddress e1 = EnmxAddress.Parse(a1);
            Assert.IsTrue(e1 == 3102);
            int i1 = e1.Value;
            EnmxAddress e2 = new EnmxAddress(i1);
            Assert.AreEqual(a1, e2.Address);
            for (int i = 0; i < 32768; i++)
            {
                EnmxAddress a = new EnmxAddress(i);
                string aString = a.Address;
                EnmxAddress b = EnmxAddress.Parse(aString);
                Assert.AreEqual(a, b);
                Assert.IsTrue(a == b);
            }
        }
    }
}
