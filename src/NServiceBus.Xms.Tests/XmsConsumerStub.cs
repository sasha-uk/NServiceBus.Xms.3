using System;

namespace NServiceBus.Xms.Tests
{
    public class XmsConsumerStub : IXmsConsumer
    {
        private Func<IBM.XMS.IMessage> receiveNoWaitAction = () => null;
        private Func<int, IBM.XMS.IMessage> receiveActionMilliseonds = x => null;
        private Func<IBM.XMS.IMessage> receiveAction;

        public void Dispose()
        {
        }

        public XmsConsumerStub StubReceiveNoWait(Func<IBM.XMS.IMessage> action)
        {
            receiveNoWaitAction = action;
            return this;
        }

        public IBM.XMS.IMessage ReceiveNoWait()
        {
            return receiveNoWaitAction();
        }

        public XmsConsumerStub StubReceive(Func<int, IBM.XMS.IMessage> action)
        {
            receiveActionMilliseonds = action;
            return this;
        }

        public IBM.XMS.IMessage Receive(int milisecondsToWaitForMessage)
        {
            return receiveActionMilliseonds(milisecondsToWaitForMessage);
        }

        public XmsConsumerStub StubReceive(Func<IBM.XMS.IMessage> action)
        {
            receiveAction = action;
            return this;
        }

        public IBM.XMS.IMessage Receive()
        {
            return receiveAction();
        }
    }
}