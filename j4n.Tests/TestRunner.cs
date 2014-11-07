using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using j4n.Object;
using NUnit.Framework;

namespace j4n.Tests
{
    [TestFixture]
    public class TestRunner
    {
        [Test]
        public void NetStringHashMethodReturnsSameAsJava()
        {
            const int knownHashValue = 111833954;
            const string testString = "w=why";
            var hash = testString.hashCode();
            Assert.AreEqual(hash, knownHashValue);
        }
    }
}
