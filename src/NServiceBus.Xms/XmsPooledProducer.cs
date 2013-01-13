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

        public XmsPooledProducer(Pool<XmsPooledProducer> pool, IXmsProducer producer)
        {
            this.pool = pool;
            this.producer = producer;
        }

        // TODO: do we need this?
        public IXmsProducer Producer
        {
            get { return producer; }
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
            if (transaction != null)
                transaction.TransactionCompleted += (s, e) => DoDispose();
            else
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

        // this should only be called by the pool
        public void Expire()
        {
            producer.Dispose();
        }
    }
}