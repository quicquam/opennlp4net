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
    public class Class1
    {
        [Test]
        public void NetStringHashMethodReturnsSameAsJava()
        {
            const string testString = "w=why";
            var hash = testString.hashCode();
            var h = testString.GetHashCode();
            Assert.AreEqual(hash, 111833954);
        }
    }
}
