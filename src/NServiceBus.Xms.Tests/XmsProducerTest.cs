using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsProducerTest
    {
        private XmsAddress address;

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
            XmsUtilities.Purge(address);
        }

        [Test]
        public void GIVE_non_transactional_producer_WHEN_send_THEN_message_is_on_queue()
        {
            using (var producer = new XmsProducer(address, false))
            {
                var msg = producer.CreateTextMessage();
                msg.Text = "message";
                producer.Send(msg);
            }
            var after = XmsUtilities.GetCurrentQueueDebth(address);

            Assert.That(after, Is.EqualTo(1));
        }

        [Test]
        public void GIVE_transactional_producer_WHEN_send_and_commited_THEN_message_is_on_queue()
        {
            using(var scope = new TransactionScope(TransactionScopeOption.Required))
            using (var producer = new XmsProducer(address, true))
            {
                var msg = producer.CreateTextMessage();
                msg.Text = "message";
                producer.Send(msg);
                scope.Complete();
            }
            var after = XmsUtilities.GetCurrentQueueDebth(address);

            Assert.That(after, Is.EqualTo(1));
        }

        [Test]
        public void GIVE_transactional_producer_AND_no_transaction_WHEN_send_THEN_message_is_on_queue()
        {
            using (var producer = new XmsProducer(address, true))
            {
                var msg = producer.CreateTextMessage();
                msg.Text = "message";
                producer.Send(msg);
            }
            var after = XmsUtilities.GetCurrentQueueDebth(address);

            Assert.That(after, Is.EqualTo(1));
        }

        [Test]
        public void The_same_producer_can_be_accessed_on_two_different_threads_give_trat_the_transaction_scopes_do_not_collide()
        {
            var producer = new XmsProducer(address, true);

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {    
                producer.SendTestMessage();
                scope.Complete();
            }
            var aaa = Task.Run(() =>
            {
                using (var scope2 = new TransactionScope(TransactionScopeOption.Required))
                {
                    producer.SendTestMessage();
                    scope2.Complete();
                }
            });
            aaa.Wait();
            
            var after = XmsUtilities.GetCurrentQueueDebth(address);
            Assert.That(after, Is.EqualTo(2));
        }

        [Test]
        public void GIVE_transactional_producer_WHEN_send_and_not_commited_THEN_message_is_not_on_queue()
        {
            using (new TransactionScope(TransactionScopeOption.Required)) 
            using (var producer = new XmsProducer(address, true))
            {
                var msg = producer.CreateTextMessage();
                msg.Text = "message";
                producer.Send(msg);
            }
            var after = XmsUtilities.GetCurrentQueueDebth(address);

            Assert.That(after, Is.EqualTo(0));
        }
    }
}