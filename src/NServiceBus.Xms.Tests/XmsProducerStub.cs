using System;
using IBM.XMS;

namespace NServiceBus.Xms.Tests
{
    public class XmsProducerStub:IXmsProducer
    {
        private Action<IBM.XMS.IMessage> sendAction = m =>{};
        private Func<IBytesMessage> createBytesMessageAction = () => null;
        private Func<ITextMessage> createTextMessageAction = () => null;

        public void Dispose()
        {
           
        }

        public XmsProducerStub StubSend(Action<IBM.XMS.IMessage> action)
        {
            sendAction = action;
            return this;
        }

        public void Send(IBM.XMS.IMessage message)
        {
            sendAction(message);
        }

        public XmsProducerStub StubCreateBytesMessage(Func<IBytesMessage> action)
        {
            createBytesMessageAction = action;
            return this;
        }

        public IBytesMessage CreateBytesMessage()
        {
            return createBytesMessageAction();
        }

        public XmsProducerStub StubCreateTextMessage(Func<ITextMessage> action)
        {
            createTextMessageAction = action;
            return this;
        }

        public ITextMessage CreateTextMessage()
        {
            return createTextMessageAction();
        }

    }
}