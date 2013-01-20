using System;
using System.Transactions;
using IBM.XMS;
using NServiceBus.Xms.Pooling;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsPooledProducer : IXmsProducer, IExpirable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(XmsPooledProducer));
        private readonly Pool<XmsPooledProducer> pool;
        private readonly IXmsProducer producer;
        private bool faulted;
        private bool inTransaction;

        public XmsPooledProducer(Pool<XmsPooledProducer> pool, IXmsProducer producer)
        {
            this.pool = pool;
            this.producer = producer;
        }

        public void Send(IBM.XMS.IMessage message)
        {
            TrackErrors(
                () =>
                    {
                        producer.Send(message);
                        return true;
                    });
        }

        public IBytesMessage CreateBytesMessage()
        {
            return TrackErrors(() => producer.CreateBytesMessage());
        }

        public ITextMessage CreateTextMessage()
        {
            return TrackErrors(() => producer.CreateTextMessage());
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
                producer.Dispose();
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
            producer.Dispose();
        }

        private T TrackErrors<T>(Func<T> action)
        {
            try
            {
                if (Transaction.Current != null)
                {
                    inTransaction = true;
                }
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