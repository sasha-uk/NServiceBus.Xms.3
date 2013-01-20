using System;
using System.Collections.Concurrent;
using System.Transactions;
using NServiceBus.Xms.Pooling;
using NServiceBus.Xms.Utils;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsConsumerProvider : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(XmsConsumerProvider));
        private readonly bool transactional;
        private readonly ConcurrentDictionary<string, XmsPooledConsumer> threadAllocatedConsumers = new ConcurrentDictionary<string, XmsPooledConsumer>();
        private readonly ConcurrentDictionary<XmsAddress, Pool<XmsPooledConsumer>> poolsByDestination = new ConcurrentDictionary<XmsAddress, Pool<XmsPooledConsumer>>();
        
        private readonly object locker = new object();

        public XmsConsumerProvider(bool transactional)
        {
            this.transactional = transactional;
        }

        private Pool<XmsPooledConsumer> CreatePool(XmsAddress address)
        {
            log.Info("Going to create new consumer pool for address {0}".FormatWith(address));
            //TODO: make this configurable
            var store = new StackStore<XmsPooledConsumer>(60.Seconds(), 30.Seconds());
            var pool = new Pool<XmsPooledConsumer>(10, p =>
                {
                    log.Info("Going to create new plain consumer for address {0}".FormatWith(address));
                    var consumer = new XmsConsumer(address, transactional);
                    var pooled = new XmsPooledConsumer(p, consumer);
                    return pooled;
                }, store);
            return pool;
        }
        
        private bool IsInTransaction
        {
            get { return Transaction.Current != null; }
        }

        public IXmsConsumer GetConsumer(XmsAddress address)
        {
            Pool<XmsPooledConsumer> pool;
            if (!poolsByDestination.TryGetValue(address, out pool))
            {
                lock (locker)
                {
                    pool = poolsByDestination.GetOrAdd(address, CreatePool);
                }
            }

            if (IsInTransaction && transactional)
            {
                log.Debug("Detected transaction scope on transactional consumer provider. Using transaction scope allocation.");
                var transaction = Transaction.Current;
                var key = "{0}|{1}".FormatWith(transaction.TransactionInformation.LocalIdentifier, address);

                if (!threadAllocatedConsumers.ContainsKey(key))
                {
                    log.Debug("Allocating consumer to a transaction scope {0}".FormatWith(key));
                    var pooled = pool.Acquire();
                    
                    threadAllocatedConsumers[key] = pooled;
                    XmsPooledConsumer _;
                    
                    transaction.TransactionCompleted += (s, e) =>
                        {
                            log.Debug("Dealocating consumer from transaction scope {0}.".FormatWith(key));
                            threadAllocatedConsumers.TryRemove(key, out _);
                            pooled.TransactionCompleted();
                            pooled.Dispose();
                        };
                }

                return threadAllocatedConsumers[key];
            }

            log.Debug("Acquiring consumer without the transaction scope.");
            return pool.Acquire();
        }
        
        public void Dispose()
        {
            foreach (var pair in poolsByDestination)
            {
                pair.Value.Dispose();
            }
            poolsByDestination.Clear();
        }
    }
}