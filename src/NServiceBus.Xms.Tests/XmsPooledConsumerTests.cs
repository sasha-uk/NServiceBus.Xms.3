using NServiceBus.Xms.Pooling;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsPooledConsumerTests
    {
        private XmsAddress address;
        private StackStore<XmsPooledConsumer> store;

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
            XmsUtilities.Purge(address);
            store = new StackStore<XmsPooledConsumer>();
        }

        [Test]
        public void GIVEN_existing_consumer_in_pool_WHEN_aquiring_consumer_THEN_the_existing_consumer_is_returned()
        {
            var pool = new Pool<XmsPooledConsumer>(1, p => CreateFailingStub(p), store);
            Assert.That(store.Count, Is.EqualTo(0));

            XmsPooledConsumer expected;
            XmsPooledConsumer actual;
            using (var consumer = pool.Acquire())
            {
                Assert.That(store.Count, Is.EqualTo(0));
                expected = consumer;
            }

            Assert.That(store.Count, Is.EqualTo(1));

            using (var consumer = pool.Acquire())
            {
                actual = consumer;
            }

            Assert.That(store.Count, Is.EqualTo(1));
            Assert.That(actual, Is.SameAs(expected));
        }

        [Test]
        public void GIVEN_failed_consumer_THEN_it_is_not_returned_to_the_pool()
        {
            var pool = new Pool<XmsPooledConsumer>(1, p => CreateFailingStub(p), store);
            Assert.That(store.Count, Is.EqualTo(0));

            XmsPooledConsumer notExpected;
            XmsPooledConsumer actual;

            using (var consumer = pool.Acquire())
            {
                Assert.That(store.Count, Is.EqualTo(0));
                notExpected = consumer;
                Assert.Throws<TestException>(() => consumer.ReceiveNoWait());
            }

            Assert.That(store.Count, Is.EqualTo(0));

            using (var consumer = pool.Acquire())
            {
                actual = consumer;
            }

            Assert.That(store.Count, Is.EqualTo(1));
            Assert.That(actual, Is.Not.SameAs(notExpected));
        }

        [Test]
        public void GIVEN_non_transactional_pool_WHEN_send_THEN_message_is_enqueued()
        {
            using (var producer = new XmsProducer(address, false))
                producer.SendTestMessage();

            IBM.XMS.IMessage message;
            using (var pool = new Pool<XmsPooledConsumer>(2, p => new XmsPooledConsumer(p, new XmsConsumer(address, false)), store))
            using (var producer = pool.Acquire())
            {
                message = producer.ReceiveNoWait();
            }
            Assert.That(message,Is.Not.Null);
        }

        private XmsPooledConsumer CreateFailingStub(Pool<XmsPooledConsumer> pool)
        {
            var producer = new XmsConsumerStub().StubReceiveNoWait(() => { throw new TestException(); });
            var pooled = new XmsPooledConsumer(pool, producer);
            return pooled;
        }
    }
}