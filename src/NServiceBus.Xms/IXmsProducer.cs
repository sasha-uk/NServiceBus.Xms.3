using System;
using IBM.XMS;

namespace NServiceBus.Xms
{
    public interface IXmsProducer : IDisposable
    {
        void Send(IBM.XMS.IMessage message);
        IBytesMessage CreateBytesMessage();
        ITextMessage CreateTextMessage();
    }
}