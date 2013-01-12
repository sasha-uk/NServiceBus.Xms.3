using IBM.WMQ;
using IBM.XMS;
using MQC = IBM.XMS.MQC;

namespace NServiceBus.Xms
{
    public static class XmsUtilities
    {
        /// <summary>
        ///   Create a WMQ connection factory and set relevant properties.
        /// </summary>
        /// <returns>A connection factory</returns>
        public static IConnectionFactory CreateConnectionFactory(XmsAddress destination)
        {
            // Create the connection factories factory
            XMSFactoryFactory factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);

            // Use the connection factories factory to create a connection factory
            IConnectionFactory cf = factoryFactory.CreateConnectionFactory();

            // Set the properties
            cf.SetStringProperty(XMSC.WMQ_HOST_NAME, destination.Host.HostName);
            cf.SetIntProperty(XMSC.WMQ_PORT, destination.Host.Port);
            cf.SetStringProperty(XMSC.WMQ_CHANNEL, destination.Host.Channel);
            cf.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
            // since null is not permited for queue manager, pass that as empty string
            // so that, while connecting to queue manager, it finds the default queue manager.
            cf.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, destination.Host.Manager);

            //cf.SetIntProperty(XMSC.WMQ_BROKER_VERSION, Options.BrokerVersion.ValueAsNumber);

            // Integrator
            //if (Options.BrokerVersion.ValueAsNumber == XMSC.WMQ_BROKER_V2)
            //    cf.SetStringProperty(XMSC.WMQ_BROKER_PUBQ, Options.BrokerPublishQueue.Value);

            return (cf);
        }


        public static int Purge(XmsAddress destination)
        {
            int i = 0;
            var factory = CreateConnectionFactory(destination);
            using (var connection = factory.CreateConnection())
            {
                using (ISession session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge))
                {
                    IDestination queue = session.CreateQueue(destination.Queue);
                    queue.SetIntProperty(XMSC.DELIVERY_MODE, XMSC.DELIVERY_NOT_PERSISTENT);

                    using (var consumer = session.CreateConsumer(queue))
                    {
                        connection.Start();
                        while (consumer.ReceiveNoWait() != null)
                        {
                            ++i;
                        }
                    }
                }
            }
            return i;
        }

        public static int GetCurrentQueueDebth(XmsAddress address)
        {
            var manager = new MQQueueManager(address.Host.Manager, address.Host.Channel, address.Host.ConnectionName);
            var queue = manager.AccessQueue(address.Queue, MQC.MQOO_INQUIRE);
            int depth = queue.CurrentDepth;
            manager.Disconnect();
            manager.Close();
            return depth;
        }
    }
}