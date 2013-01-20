using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsMessageReceiver : IReceiveMessages
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(XmsMessageReceiver));
        private bool useTransactions;
        private XmsConsumerProvider consumerProducer;
        private XmsAddress address;

        public void Init(Address address, bool transactional)
        {
            this.address = address.ToXmsAddress();
            if (PurgeOnStartup)
                XmsUtilities.Purge(this.address);
            this.useTransactions = transactional;
            this.consumerProducer = new XmsConsumerProvider(transactional);
        }

        public bool HasMessage()
        {
            return true;
        }

        public TransportMessage Receive()
        {
            using (var consumer = consumerProducer.GetConsumer(address))
            {
                var message = consumer.Receive();
                if (message == null)
                    return null;
                var result = XmsUtilities.Convert(message);
                if (result == null)
                {
                    log.Warn("A recieved message could not be converted to transaport message. Ignoring message.");
                }
                return result;
            }
        }

        public bool PurgeOnStartup { get; set; }
    }
}