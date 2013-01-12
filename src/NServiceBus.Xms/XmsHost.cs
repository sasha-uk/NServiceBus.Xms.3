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
    }
}