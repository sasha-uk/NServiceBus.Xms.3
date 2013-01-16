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

        public XmsPooledConsumer(Pool<XmsPooledConsumer> pool, IXmsConsumer consumer)
        {
            this.pool = pool;
            this.consumer = consumer;
        }

        public IXmsConsumer Consumer
        {
            get { return consumer; }
        }

        private T TrackErrors<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch
            {
                faulted = true;
                log.Warn("Detected an error with this MQ connection. It will be disposed of and replaced with new one at the nearest oportunity.");
                throw;
            }
        }

        public void Dispose()
        {
            // we cannot return the instance into the pool before the tranaction scope completes
            var transaction = Transaction.Current;
            /*if (transaction != null)
                transaction.TransactionCompleted += (s, e) => DoDispose();
            else
                DoDispose();*/
            if (transaction == null)
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

        public IBM.XMS.IMessage ReceiveNoWait()
        {
            return TrackErrors(() => consumer.ReceiveNoWait());
        }

        public IBM.XMS.IMessage Receive(int milisecondsToWaitForMessage)
        {
            return TrackErrors(() => consumer.Receive(milisecondsToWaitForMessage));
        }
    }
}