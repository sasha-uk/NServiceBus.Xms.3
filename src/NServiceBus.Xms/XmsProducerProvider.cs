using System;
using System.Collections.Concurrent;
using System.Transactions;
using NServiceBus.Xms.Pooling;
using NServiceBus.Xms.Utils;
using log4net;

namespace NServiceBus.Xms
{
    public class XmsProducerProvider : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(XmsProducerProvider));
        private readonly bool transactional;
        private readonly ConcurrentDictionary<string, XmsPooledProducer> threadAllocatedProducers = new ConcurrentDictionary<string, XmsPooledProducer>();
        private readonly ConcurrentDictionary<XmsAddress, Pool<XmsPooledProducer>> poolsByDestination = new ConcurrentDictionary<XmsAddress, Pool<XmsPooledProducer>>();
        
        private readonly object locker = new object();

        public XmsProducerProvider(bool transactional)
        {
            this.transactional = transactional;
        }

        private Pool<XmsPooledProducer> CreatePool(XmsAddress address)
        {
            log.Info("Going to create new producer pool for address {0}".FormatWith(address));
            //TODO: make this configurable
            var store = new StackStore<XmsPooledProducer>(60.Seconds(), 30.Seconds());
            var pool = new Pool<XmsPooledProducer>(10, p =>
                {
                    log.Info("Going to create new plain producer for address {0}".FormatWith(address));
                    var producer = new XmsProducer(address, transactional);
                    var pooled = new XmsPooledProducer(p, producer);
                    return pooled;
                }, store);
            return pool;
        }
        
        private bool IsInTransaction
        {
            get { return Transaction.Current != null; }
        }

        public IXmsProducer GetProducer(XmsAddress address)
        {
            Pool<XmsPooledProducer> pool;
            if (!poolsByDestination.TryGetValue(address, out pool))
            {
                lock (locker)
                {
                    pool = poolsByDestination.GetOrAdd(address, CreatePool);
                }
            }

            if (IsInTransaction && transactional)
            {
                log.Debug("Detected transaction scope on transactional producer provider. Using transaction scope allocation.");
                var transaction = Transaction.Current;
                var key = "{0}|{1}".FormatWith(transaction.TransactionInformation.LocalIdentifier, address);

                if (!threadAllocatedProducers.ContainsKey(key))
                {
                    log.Debug("Allocating producer to a transaction scope {0}".FormatWith(key));
                    var pooled = pool.Acquire();
                    threadAllocatedProducers[key] = pooled;
                    XmsPooledProducer _;
                    transaction.TransactionCompleted += (s, e) =>
                        {
                            log.Debug("Dealocating producer from transaction scope {0}.".FormatWith(key));
                            threadAllocatedProducers.TryRemove(key, out _);
                        };
                }

                return threadAllocatedProducers[key];
            }

            log.Debug("Acquiring producer without the transaction scope.");
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