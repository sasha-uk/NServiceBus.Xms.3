using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.Xms
{
    public class XmsMessageReceiver : IReceiveMessages
    {
        public void Init(Address address, bool transactional)
        {
        }

        public bool HasMessage()
        {
            return false;
        }

        public TransportMessage Receive()
        {
            return null;
        }

        public bool PurgeOnStartup { get; set; }
    }
}