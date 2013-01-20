using System.Transactions;
using NServiceBus.Xms.Pooling;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsPooledProducerTests
    {
        private XmsAddress address;
        private StackStore<XmsPooledProducer> store;

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
            XmsUtilities.Purge(address);
            store = new StackStore<XmsPooledProducer>();
        }

        [Test]
        public void GIVEN_existing_producer_in_pool_WHEN_aquiring_producer_THEN_the_existing_producer_is_returned()
        {
            var pool = new Pool<XmsPooledProducer>(1, p => CreateFailingStub(p), store);
            Assert.That(store.Count, Is.EqualTo(0));

            XmsPooledProducer expected;
            XmsPooledProducer actual;
            using (var producer = pool.Acquire())
            {
                Assert.That(store.Count, Is.EqualTo(0));
                expected = producer;
            }
            
            Assert.That(store.Count, Is.EqualTo(1));

            using (var producer = pool.Acquire())
            {
                actual = producer;
            }

            Assert.That(store.Count, Is.EqualTo(1));
            Assert.That(actual, Is.SameAs(expected));
        }


        /*[Test]
        public void GIVEN_existing_producer_in_pool_WHEN_aquiring_producer_THEN_the_existing_producer_is_returned2()
        {
            var pool = new Pool<XmsPooledProducer>(1, p => CreateFailingStub(p), store);
            Assert.That(store.Count, Is.EqualTo(0));

            XmsPooledProducer expected;
            XmsPooledProducer actual;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                using (var producer = pool.Acquire())
                {
                    producer.SendTestMessage(address);
                }
                using (var producer = pool.Acquire())
                {
                    producer.SendTestMessage(address);
                }
                scope.Complete();
            }
            Assert.That(store.Count, Is.EqualTo(2));
        }*/

        [Test]
        public void GIVEN_failed_producer_THEN_it_is_not_returned_to_the_pool()
        {
            var pool = new Pool<XmsPooledProducer>(1, p => CreateFailingStub(p), store);
            Assert.That(store.Count, Is.EqualTo(0));

            XmsPooledProducer notExpected;
            XmsPooledProducer actual;

            using (var producer = pool.Acquire())
            {
                Assert.That(store.Count, Is.EqualTo(0));
                notExpected = producer;
                Assert.Throws<TestException>(() => producer.Send(null));
            }

            Assert.That(store.Count, Is.EqualTo(0));
            
            using (var producer = pool.Acquire())
            {
                actual = producer;
            }

            Assert.That(store.Count, Is.EqualTo(1));
            Assert.That(actual, Is.Not.SameAs(notExpected));
        }

        [Test]
        public void GIVEN_non_transactional_pool_WHEN_send_THEN_message_is_enqueued()
        {
            using (var pool = new Pool<XmsPooledProducer>(2, p => new XmsPooledProducer(p, new XmsProducer(address, false)), store))
            using (var producer = pool.Acquire())
            {
                producer.SendTestMessage();
            }
            address.AssertMessageCount(1);
        }
        
        private XmsPooledProducer CreateFailingStub(Pool<XmsPooledProducer> pool)
        {
            var producer = new XmsProducerStub().StubSend(m => { throw new TestException(); });
            var pooled = new XmsPooledProducer(pool, producer);
            return pooled;
        }
    }
}