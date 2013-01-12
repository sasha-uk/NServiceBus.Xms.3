using System.Transactions;
using IBM.XMS;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsConsumerTest
    {
        private XmsAddress address;
        private const string expected = "foo";

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
            XmsUtilities.Purge(address);

            using (var producer = new XmsProducer(address, false))
            {
                var msg = producer.CreateBytesMessage();
                msg.WriteUTF(expected);
                producer.Send(msg);
            }
        }

        [Test]
        public void GIVEN_non_transactional_consumer_AND_existing_message_WHEN_receive_no_wait_THEN_message_should_be_dequeued()
        {
            string actual;
            using (var consumer = new XmsConsumer(address, false))
            {
                var msg = (IBytesMessage)consumer.ReceiveNoWait();
                actual = msg.ReadUTF();
            }
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(XmsUtilities.GetCurrentQueueDebth(address), Is.EqualTo(0), "Queue should be empty");
        }

        [Test]
        public void GIVEN_non_transactional_consumer_AND_existing_message_WHEN_receive_with_wait_THEN_message_should_be_dequeued()
        {
            string actual;
            using (var consumer = new XmsConsumer(address, false))
            {
                var msg = (IBytesMessage)consumer.Receive(10);
                actual = msg.ReadUTF();
            }
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(XmsUtilities.GetCurrentQueueDebth(address), Is.EqualTo(0), "Queue should be empty");
        }


        [Test]
        public void GIVEN_transactional_consumer_AND_existing_message_WHEN_receive_AND_scope_commited_THEN_message_should_be_dequeued()
        {
            string actual;

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            using (var consumer = new XmsConsumer(address, true))
            {
                var msg = (IBytesMessage)consumer.ReceiveNoWait();
                actual = msg.ReadUTF();
                Assert.That(msg, Is.Not.Null);
                scope.Complete();
            }

            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(XmsUtilities.GetCurrentQueueDebth(address), Is.EqualTo(0), "Queue should be empty");
        }

        [Test]
        public void GIVEN_transactional_consumer_AND_existing_message_WHEN_receive_AND_scope_disposed_THEN_message_should_be_dequeued()
        {
            string actual;

            using (new TransactionScope(TransactionScopeOption.Required)) 
            using (var consumer = new XmsConsumer(address, true))
            {
                var msg = (IBytesMessage)consumer.ReceiveNoWait();
                actual = msg.ReadUTF();
                Assert.That(msg, Is.Not.Null);
            }

            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(XmsUtilities.GetCurrentQueueDebth(address), Is.EqualTo(1), "Message should be returned back to the queue.");
        }
    }
}