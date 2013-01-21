using System;
using IBM.XMS;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsConsumer : IXmsConsumer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (XmsProducer));
        private readonly XmsAddress address;
        private readonly bool transactional;
        private IConnectionFactory factory;
        private IConnection connection;
        private ISession session;
        private IDestination queue;
        private IMessageConsumer consumer;
        private bool connected;

        public XmsConsumer(XmsAddress address, bool transactional)
        {
            this.address = address;
            this.transactional = transactional;
        }

        public void Connect()
        {
            factory = XmsUtilities.CreateConnectionFactory(address);
            connection = factory.CreateConnection();
            session = connection.CreateSession(transactional, AcknowledgeMode.AutoAcknowledge);
            queue = session.CreateQueue(address.Queue);
            queue.SetIntProperty(XMSC.DELIVERY_MODE, transactional ? XMSC.DELIVERY_PERSISTENT : XMSC.DELIVERY_NOT_PERSISTENT);
            consumer = session.CreateConsumer(queue);
            connection.Start();
            connected = true;
        }

        public IBM.XMS.IMessage ReceiveNoWait()
        {
            if (!connected) Connect();
            var message = consumer.ReceiveNoWait();
            return message;
        }

        public IBM.XMS.IMessage Receive(int milisecondsToWaitForMessage)
        {
            if (!connected) Connect();
            if (milisecondsToWaitForMessage < 0)
                return consumer.Receive();
            return consumer.Receive(milisecondsToWaitForMessage);
        }

        public IBM.XMS.IMessage Receive()
        {
            if (!connected) Connect();
            return consumer.Receive();
        }

        private void Disconnect()
        {
            log.Debug("Physical consumer about to be disconnected.");

            if (connection != null) connection.Stop();
            if (consumer != null) consumer.Dispose();
            if (queue != null) queue.Dispose();
            if (session != null) session.Dispose();
            if (connection != null) connection.Dispose();

            consumer = null;
            queue = null;
            session = null;
            connection = null;

            log.Debug("Physical consumer successfully disconnected.");
        }

        public void Dispose()
        {
            Disconnect();
        }        
    }
}