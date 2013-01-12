using System;
using IBM.XMS;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsProducer : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (XmsProducer));
        private readonly XmsAddress address;
        private readonly bool transactional;
        private IConnectionFactory factory;
        private IConnection connection;
        private ISession session;
        private IDestination queue;
        private IMessageProducer producer;
        private bool connected;

        public XmsProducer(XmsAddress address, bool transactional)
        {
            this.address = address;
            this.transactional = transactional;
        }

        public void Connect()
        {
            log.Debug("New physical producer created. About to connect.");
            factory = XmsUtilities.CreateConnectionFactory(address);
            connection = factory.CreateConnection();
            connection.ExceptionListener += OnError;
            session = connection.CreateSession(transactional, AcknowledgeMode.AutoAcknowledge);
            queue = session.CreateQueue(address.Queue);
            queue.SetIntProperty(XMSC.DELIVERY_MODE, XMSC.DELIVERY_PERSISTENT);
            producer = session.CreateProducer(queue);
            connected = true;
            log.Debug("New physical producer successfully connected.");
        }

        public void Send(IBM.XMS.IMessage message)
        {
            if (!connected) Connect();
            producer.Send(message);
        }

        public void Disconnect()
        {
            log.Debug("Physical producer about to be disconnected.");

            if (producer != null) producer.Dispose();
            if (queue != null) queue.Dispose();
            if (session != null) session.Dispose();
            if (connection != null) connection.Dispose();

            producer = null;
            queue = null;
            session = null;
            connection = null;

            log.Debug("Physical producer successfully disconnected.");
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void OnError(Exception ex)
        {
            log.Error(ex);
        }

        public ITextMessage CreateTextMessage()
        {
            if (!connected) Connect();
            return session.CreateTextMessage();
        }

        public IBytesMessage CreateBytesMessage()
        {
            if (!connected) Connect();
            return session.CreateBytesMessage();
        }
    }
}