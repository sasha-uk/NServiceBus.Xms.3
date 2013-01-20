using System;
using System.Transactions;
using NServiceBus.Xms.Pooling;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsPooledConsumer : IXmsConsumer, IExpirable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(XmsPooledConsumer));
        private readonly Pool<XmsPooledConsumer> pool;
        private readonly IXmsConsumer consumer;
        private bool faulted;
        private bool inTransaction;

        public XmsPooledConsumer(Pool<XmsPooledConsumer> pool, IXmsConsumer consumer)
        {
            this.pool = pool;
            this.consumer = consumer;
        }

        public IBM.XMS.IMessage ReceiveNoWait()
        {
            return TrackErrors(() => consumer.ReceiveNoWait());
        }

        public IBM.XMS.IMessage Receive(int milisecondsToWaitForMessage)
        {
            return TrackErrors(() => consumer.Receive(milisecondsToWaitForMessage));
        }

        public IBM.XMS.IMessage Receive()
        {
            return TrackErrors(() => consumer.Receive());
        }

        public void Dispose()
        {
            if (!inTransaction)
                DoDispose();
        }

        private void DoDispose()
        {
            if (pool.IsDisposed)
            {
                consumer.Dispose();
                return;
            }

            if (faulted)
            {
                pool.Release(null);
                return;
            }

            pool.Release(this);
        }

        public void Expire()
        {
            consumer.Dispose();
        }

        private T TrackErrors<T>(Func<T> action)
        {
            try
            {
                if (Transaction.Current != null)
                    inTransaction = true;
                return action();
            }
            catch
            {
                faulted = true;
                log.Warn("Detected an error with this MQ connection. It will be disposed of and replaced with new one at the nearest oportunity.");
                throw;
            }
        }

        public void TransactionCompleted()
        {
            inTransaction = false;
        }
    }
}