using System;
using System.Collections.Generic;
using System.Linq;
using IBM.WMQ;
using IBM.XMS;
using NServiceBus.Unicast.Transport;
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

        private static readonly string HEADER_RETURNADDRESS = "ReturnAddress";
        private static readonly string HEADER_IDFORCORRELATION = "CorrId";
        //private static readonly string HEADER_WINDOWSIDENTITYNAME = "WinIdName";
        private static readonly string HEADER_MESSAGEINTENT = "MessageIntent";
        private static readonly string HEADER_NBSKEYS = "NSBKeys";
        //private static readonly string HEADER_FAILEDQUEUE = "FailedQ";
        private static readonly string HEADER_ORIGINALID = "OriginalId";

        public static void Convert(TransportMessage message, IBytesMessage toSend)
        {
            if (message.Body != null)
                toSend.WriteBytes(message.Body);

            // TODO: clarify usage of JMSCorrelationID
            
            if (message.CorrelationId != null)
                toSend.JMSCorrelationID = message.CorrelationId;
            if (message.ReplyToAddress != null)
                toSend.SetStringProperty(HEADER_RETURNADDRESS, message.ReplyToAddress.ToString());
            if (message.IdForCorrelation != null)
                toSend.SetStringProperty(HEADER_IDFORCORRELATION, message.IdForCorrelation ?? string.Empty);

            toSend.JMSDeliveryMode = message.Recoverable ? DeliveryMode.Persistent : DeliveryMode.NonPersistent;
            //toSend.SetStringProperty(HEADER_WINDOWSIDENTITYNAME, message.WindowsIdentityName);
            toSend.SetIntProperty(HEADER_MESSAGEINTENT, (int)message.MessageIntent);

            //TODO: set message expiration
            //toSend.JMSReplyTo = new Destination message.ReplyToAddress;
            //if (message.TimeToBeReceived < MessageQueue.InfiniteTimeout)
            //toSend.JMSExpiration = (long) UTCNow.message.TimeToBeReceived.TotalMilliseconds;
            //if (message.TimeToBeReceived < MessageQueue.InfiniteTimeout)
            //toSend.TimeToBeReceived = message.TimeToBeReceived;

            if (message.Headers == null)
                message.Headers = new Dictionary<string, string>();
            if (!message.Headers.ContainsKey("CorrId"))
                message.Headers.Add("CorrId", null);
            if (string.IsNullOrEmpty(message.Headers["CorrId"]))
                message.Headers["CorrId"] = message.IdForCorrelation ?? string.Empty;

            var nsbHeaderKeys = new List<string>();
            foreach (var keyValue in message.Headers)
            {
                toSend.SetStringProperty(keyValue.Key.ToXmsFriendly(), keyValue.Value);
                nsbHeaderKeys.Add(keyValue.Key.ToXmsFriendly());
            }
            toSend.SetStringProperty(HEADER_NBSKEYS, WrapKeys(nsbHeaderKeys));
        }

        public static TransportMessage Convert(IBM.XMS.IMessage m)
        {
            var result = new TransportMessage();
            result.Id = GetRealMessageId(m);
            result.CorrelationId = m.JMSCorrelationID;
            result.Recoverable = m.JMSDeliveryMode == DeliveryMode.Persistent;
            result.IdForCorrelation = m.GetStringProperty(HEADER_IDFORCORRELATION);
            result.ReplyToAddress = m.GetStringProperty(HEADER_RETURNADDRESS).ToXmsAddress().ToNsbAddress();
            //result.WindowsIdentityName = m.GetStringProperty(HEADER_WINDOWSIDENTITYNAME);
            result.MessageIntent = (MessageIntentEnum)m.GetIntProperty(HEADER_MESSAGEINTENT);
            //result.TimeSent = baseDate.AddMilliseconds(m.JMSTimestamp);
            result.Headers = new Dictionary<string, string>();
            //TODO:
            //result.TimeToBeReceived = DateTime.UtcNow - baseDate.AddMilliseconds(m.JMSExpiration);
            if (m.GetStringProperty("NSBKeys") != null)
            {
                var keys = UnwrapKeys(m.GetStringProperty("NSBKeys"));

                result.Headers = (from k in keys
                                  select new {Key = k.FromXmsFriendly(), Value = m.GetStringProperty(k)})
                                  .ToDictionary(x=>x.Key,x=>x.Value);
            }

            //TODO:
            //TimeToBeReceived = baseDate.AddMilliseconds(m.JMSTimestamp),
            //ReplyToAddress = GetIndependentAddressForQueue(m.ResponseQueue),
            var byteMessage = m as IBytesMessage;
            if (byteMessage == null)
            {
                return null;
            }
            if (byteMessage.BodyLength > 0)
            {
                var body = new byte[byteMessage.BodyLength];
                byteMessage.ReadBytes(body);
                result.Body = body;
            }
            return result;

/*
            TransportMessage transportMessage = new TransportMessage()
            {
                Id = m.Id,
                CorrelationId = m.CorrelationId == "00000000-0000-0000-0000-000000000000\\0" ? (string)null : m.CorrelationId,
                Recoverable = m.Recoverable,
                TimeToBeReceived = m.TimeToBeReceived,
                TimeSent = m.SentTime,
                ReplyToAddress = MsmqUtilities.GetIndependentAddressForQueue(m.ResponseQueue),
                MessageIntent = Enum.IsDefined(typeof(MessageIntentEnum), (object)m.AppSpecific) ? (MessageIntentEnum)m.AppSpecific : MessageIntentEnum.Send
            };
            m.BodyStream.Position = 0L;
            transportMessage.Body = new byte[m.BodyStream.Length];
            m.BodyStream.Read(transportMessage.Body, 0, transportMessage.Body.Length);
            transportMessage.Headers = new Dictionary<string, string>();
            if (m.Extension.Length > 0)
            {
                MemoryStream memoryStream = new MemoryStream(m.Extension);
                foreach (HeaderInfo headerInfo in MsmqUtilities.headerSerializer.Deserialize((Stream)memoryStream) as List<HeaderInfo>)
                {
                    if (headerInfo.Key != null)
                        transportMessage.Headers.Add(headerInfo.Key, headerInfo.Value);
                }
            }
            transportMessage.Id = TransportHeaderKeys.GetOriginalId(transportMessage);
            if (transportMessage.Headers.ContainsKey("EnclosedMessageTypes"))
                MsmqUtilities.ExtractMsmqMessageLabelInformationForBackwardCompatibility(m, transportMessage);
            transportMessage.IdForCorrelation = TransportHeaderKeys.GetIdForCorrelation(transportMessage);
            return transportMessage;*/

        }

        //private static DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string GetRealMessageId(IBM.XMS.IMessage message)
        {
            var id = message.GetStringProperty(HEADER_ORIGINALID);
            return string.IsNullOrEmpty(id) ? message.JMSMessageID : id;
        }

        
        public static string ToXmsFriendly(this string value)
        {
            return value.Replace(".", "_");
        }

        public static string FromXmsFriendly(this string value)
        {
            return value.Replace("_", ".");
        }

        private static string WrapKeys(IEnumerable<string> keys)
        {
            return string.Join("|", keys);
        }

        private static string[] UnwrapKeys(string keys)
        {
            return keys.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}