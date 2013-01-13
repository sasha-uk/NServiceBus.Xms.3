using System;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.Xms
{
    public class XmsMessageSender : ISendMessages
    {
        private XmsProducerProvider provider;

        public XmsMessageSender(bool transactional)
        {
            provider = new XmsProducerProvider(transactional);
        }
        
        public bool Transactional { get; set; }
        // TODO: remove this
        public bool UseJournalQueue { get; set; }
        // TODO: remove this
        public bool UseDeadLetterQueue { get; set; }

        public void Send(TransportMessage message, Address address)
        {
            try
            {
                XmsAddress addr = address.ToXmsAddress();
                using (var producer = provider.GetProducer(addr))
                {
                    var xmsMessage = producer.CreateBytesMessage();
                    XmsUtilities.Convert(message, xmsMessage);
                    producer.Send(xmsMessage);
                    message.Id = xmsMessage.JMSMessageID;
                   
                }
            }
            // TODO: QueueNotFoundException
            catch (Exception ex)
            {
                if (address == null)
                    throw new FailedToSendMessageException("Failed to send message.", ex);

                throw new FailedToSendMessageException(
                    string.Format("Failed to send message to address: {0}@{1}", address.Queue, address.Machine), ex);
            }
         
        }
        
        /*     void ISendMessages.Send(TransportMessage message, Address address)
        {
            var queuePath = MsmqUtilities.GetFullPath(address);
            try
            {
                using (var q = new MessageQueue(queuePath, false, true, QueueAccessMode.Send))
                {
                    using (Message toSend = MsmqUtilities.Convert(message))
                    {
                        toSend.UseDeadLetterQueue = UseDeadLetterQueue;
                        toSend.UseJournalQueue = UseJournalQueue;

                        if (message.ReplyToAddress != null)
                            toSend.ResponseQueue = new MessageQueue(MsmqUtilities.GetReturnAddress(message.ReplyToAddress.ToString(), address.ToString()));


                        q.Send(toSend, GetTransactionTypeForSend());

                        message.Id = toSend.Id;
                    }
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode == MessageQueueErrorCode.QueueNotFound)
                {
                    var msg = address == null
                                     ? "Failed to send message. Target address is null."
                                     : string.Format("Failed to send message to address: [{0}]", address);

                    throw new QueueNotFoundException(address, msg, ex);
                }

                ThrowFailedToSendException(address, ex);
            }
            catch (Exception ex)
            {
                ThrowFailedToSendException(address, ex);
            }
        }

        private static void ThrowFailedToSendException(Address address, Exception ex)
        {
            if (address == null)
                throw new FailedToSendMessageException("Failed to send message.", ex);

            throw new FailedToSendMessageException(
                string.Format("Failed to send message to address: {0}@{1}", address.Queue, address.Machine), ex);
        }

        private static MessageQueueTransactionType GetTransactionTypeForSend()
        {
            return Transaction.Current != null ? MessageQueueTransactionType.Automatic : MessageQueueTransactionType.Single;
        }*/
    }
}

