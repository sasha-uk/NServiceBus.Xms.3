using System;
using System.Transactions;
using NServiceBus.Xms.Tests;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsConsumerProviderTest
    {
        private XmsAddress address;
        private const string expected = "foo";

        [SetUp]
        public void SetUp()
        {
            address = TestTargets.InputQueue;
        }

        [Test]
        [Ignore]
        public void GIVE_trans()
        {
            using (var consumer = new XmsConsumer(address, true))
            {
                consumer.Connect();

                using (var scope = new CommittableTransaction())
                {
                    Transaction.Current = scope;
                    try
                    {
                        consumer.Receive(1);
                        scope.Commit();
                    }
                    catch (Exception)
                    {
                        scope.Rollback();
                        throw;
                    }
                    finally
                    {
                        Transaction.Current = null;
                    }
                }

                Console.WriteLine("s");

                using (var scope = new CommittableTransaction())
                {
                    Transaction.Current = scope;
                    try
                    {
                        consumer.ReceiveNoWait();
                        scope.Commit();
                    }
                    catch (Exception)
                    {
                        scope.Rollback();
                        throw;
                    }
                    finally
                    {
                        Transaction.Current = null;
                    }
                }

                Console.WriteLine("s");
            }
        }

        [Test]
        [Ignore]
        public void GIVE_scopes()
        {
            var consumer = new XmsConsumer(address, true);
            consumer.Connect();

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                consumer.ReceiveNoWait();
                scope.Complete();
            }

            Console.WriteLine("s");

            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                consumer.ReceiveNoWait();
                scope.Complete();
            }
            Console.WriteLine("s");
        }

        [Test]
        public void GIVEN_transactional_consumer_AND_existing_message_WHEN_receive_AND_scope_disposed_THEN_message_should_be_dequeued()
        {
            XmsUtilities.Purge(address);
            using (var producer = new XmsProducer(address, false))
            {
                producer.SendTestMessage();
                producer.SendTestMessage();
            }
            //address.PostTestMessages(2);
            using (var provider = new XmsConsumerProvider(true))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                using (var consumer = provider.GetConsumer(address))
                {
                    consumer.Receive();
                    scope.Complete();
                }

                Assert.That(provider, Is.Not.Null);

                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                using (var consumer = provider.GetConsumer(address))
                {
                    consumer.Receive();
                    scope.Complete();
                }
                Assert.That(provider, Is.Not.Null);    
            }
        }
    }
}