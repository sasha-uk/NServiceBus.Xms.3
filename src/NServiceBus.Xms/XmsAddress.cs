using System;
using NServiceBus.Xms.Utils;

namespace NServiceBus.Xms
{
    public class XmsAddress
    {
        public string Queue { get; private set; }
        public XmsHost Host { get; private set; }

        public XmsAddress(string queue, XmsHost host)
        {
            Queue = queue;
            Host = host;
        }

        public override string ToString()
        {
            return "{0}@{1}/{2}/{3}/{4}".FormatWith(Queue, Host.Manager, Host.HostName, Host.Port, Host.Channel);
        }
    }

    public static class XmsAddressExtensions
    {
        public static XmsAddress ToXmsAddress(this Address address)
        {
            return address.ToString().ToXmsAddress();
        }

        public static XmsAddress ToXmsAddress(this string address)
        {
            try
            {
                var parts = address.ToUpper().Split('@');

                var queueName = parts[0];
                var args = parts[1].Split('/');
                var queueManager = args[0];
                var hostName = args[1];
                var port = int.Parse(args[2]);
                var channel = args.Length > 3 ? args[3] : string.Empty;

                return new XmsAddress(queueName, new XmsHost(queueManager, hostName, port, channel));
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to parse {0} to xms address.".FormatWith(address), ex);
            }
        }
    }
}