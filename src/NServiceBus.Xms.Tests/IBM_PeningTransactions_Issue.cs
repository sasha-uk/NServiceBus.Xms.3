using System.Transactions;
using IBM.XMS;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    /// <summary>
    /// https://www.ibm.com/developerworks/forums/thread.jspa?threadID=468510&tstart=0
    /// </summary>
    [TestFixture]
    public class IBM_PeningTransactions_Issue
    {
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
            var session = connection.CreateSession(true, AcknowledgeMode.SessionTransacted);
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
    }
}