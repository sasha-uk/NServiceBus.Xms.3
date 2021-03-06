﻿using System;
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
            return "{0}@{1}".FormatWith(Queue, Host);
        }

        protected bool Equals(XmsAddress other)
        {
            return string.Equals(Queue, other.Queue) && Equals(Host, other.Host);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((XmsAddress) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Queue != null ? Queue.GetHashCode() : 0)*397) ^ (Host != null ? Host.GetHashCode() : 0);
            }
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
            if (address == null) return null;
            try
            {
                var parts = address.ToUpper().Split('@');

                var queueName = parts[0];
                var args = parts[1].Split(XmsHost.Separator);
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

        public static Address ToNsbAddress(this XmsAddress address)
        {
            if (address == null) return null;
            try
            {
                return new Address(address.Queue, address.Host.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to parse {0} to xms address.".FormatWith(address), ex);
            }
        }
    }
}