namespace NServiceBus.Xms
{
    public class XmsHost
    {
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public string Channel { get; private set; }
        public string Manager { get; private set; }

        public XmsHost(string manager, string hostName, int port, string channel)
        {
            Manager = manager;
            Channel = channel;
            Port = port;
            HostName = hostName;
        }

        public string ConnectionName
        {
            get { return string.Format("{0}({1})", HostName, Port); }
        }

        protected bool Equals(XmsHost other)
        {
            return string.Equals(HostName, other.HostName) && Port == other.Port && string.Equals(Channel, other.Channel) && string.Equals(Manager, other.Manager);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((XmsHost)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (HostName != null ? HostName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Port;
                hashCode = (hashCode * 397) ^ (Channel != null ? Channel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Manager != null ? Manager.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}