using System.Transactions;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsProducerProviderTest
    {
        private XmsProducerProvider provider;
        private XmsAddress address;

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
            XmsUtilities.Purge(address);
            provider = new XmsProducerProvider(true);
        }

        [Test]
        public void GIVEN_there_are_no_consumers_THEN_one_will_be_created()
        {
            var producer = provider.GetProducer(address);
            Assert.That(producer, Is.Not.Null);
        }

        [Test]
        public void GIVEN_not_transactional_scope_AND_single_thread_THEN_it_will_get_the_same_producer()
        {
            var producer1 = provider.GetProducer(address);
            producer1.Dispose();
            var producer2 = provider.GetProducer(address);
            Assert.That(producer1,Is.SameAs(producer2));
        }

        [Test]
        public void GIVEN_transactional_scope_AND_single_thread_THEN_it_will_get_the_same_producer()
        {
            using (new TransactionScope(TransactionScopeOption.Required))
            {
                var producer1 = provider.GetProducer(address);
                var producer2 = provider.GetProducer(address);
                Assert.That(producer1, Is.SameAs(producer2));
            }
        }
    }
}