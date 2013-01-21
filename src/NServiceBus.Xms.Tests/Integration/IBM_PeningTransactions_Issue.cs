using System.Linq;
using System.Transactions;
using IBM.XMS;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests.Integration
{
  
    [TestFixture]
    public class IBM_PeningTransactions_Issue
    {
        private readonly XmsAddress address = TestTargets.InputQueue;

        /// <summary>
        /// https://www.ibm.com/developerworks/forums/thread.jspa?threadID=468510&tstart=0
        /// </summary>
        [Test]
        public void PendingTransactions()
        {
            var factory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
            var cf = factory.CreateConnectionFactory();
            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, TestTargets.InputQueue.Host.HostName);
            cf.SetIntProperty(XMSC.WMQ_PORT, TestTargets.InputQueue.Host.Port);
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, TestTargets.InputQueue.Host.Channel);
            cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, TestTargets.InputQueue.Host.Manager);

            var connection = cf.CreateConnection();
            var session = connection.CreateSession(true, AcknowledgeMode.AutoAcknowledge);
            var queue = session.CreateQueue(TestTargets.InputQueue.Queue);
            queue.SetIntProperty(XMSC.DELIVERY_MODE, XMSC.DELIVERY_PERSISTENT);
            var consumer = session.CreateConsumer(queue);
            connection.Start();
            
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                consumer.Receive(1);
                scope.Complete();
            }
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                consumer.Receive(1);
                scope.Complete();
            }
        }

        /// <summary>
        /// NOTE: This leaves a pending DTC transaction behind
        /// </summary>
        [Test]
        public void ReceiveMessageFromEmptyQueue()
        {
            XmsUtilities.Purge(address);

            var consumer = new XmsConsumer(address, true);
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                consumer.Receive(1);
                scope.Complete();
            }
        }

        /// <summary>
        /// NOTE: This fails after a few messages have been dequeued - usually less than 20
        /// </summary>
        [Test]
        public void SendReceive100()
        {
            XmsUtilities.Purge(address);
            foreach (var i in Enumerable.Range(0, 100))
            {
                using (var producer = new XmsProducer(address, false))
                {
                    producer.SendTestMessage();
                }
            }

            var consumer = new XmsConsumer(address, true);

            foreach (var i in Enumerable.Range(0, 100))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    // expect error here
                    consumer.Receive();
                    scope.Complete();
                }
            }
        }

        
    }
}