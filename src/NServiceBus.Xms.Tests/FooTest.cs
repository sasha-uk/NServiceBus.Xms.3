using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class FooTest
    {
        [Test]
        public void TestFoo()
        {
            Assert.That(new Foo(), Is.Not.Null);
        }
    }
}