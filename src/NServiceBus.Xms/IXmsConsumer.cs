using System;

namespace NServiceBus.Xms
{
    public interface IXmsConsumer : IDisposable
    {
        IBM.XMS.IMessage ReceiveNoWait();
        IBM.XMS.IMessage Receive(int milisecondsToWaitForMessage);
    }
}