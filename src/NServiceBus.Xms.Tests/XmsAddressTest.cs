using System;
using IBM.XMS;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsAddressTest
    {
        [Test]
        public void WHEN_ToString_THEN_returns_correctly_formated_string()
        {
            var address = new XmsAddress("QUEUE", new XmsHost("MANAGER", "HOSTNAME", 42, "CHANNEL"));
            Assert.That(address.ToString(), Is.EqualTo("QUEUE@MANAGER:HOSTNAME:42:CHANNEL"));
        }

        [Test]
        public void GIVEN_existing_ns_address_WHEN_parsed_to_xms_THEN_values_match()
        {
            var address = new Address("QUEUE", "MANAGER:HOSTNAME:42:CHANNEL");
            var xms = address.ToXmsAddress();
            Assert.That(xms.Queue, Is.EqualTo("QUEUE"));
            Assert.That(xms.Host.Manager, Is.EqualTo("MANAGER"));
            Assert.That(xms.Host.HostName, Is.EqualTo("HOSTNAME"));
            Assert.That(xms.Host.Port, Is.EqualTo(42));
            Assert.That(xms.Host.Channel, Is.EqualTo("CHANNEL"));
            Assert.That(xms.Host.ConnectionName, Is.EqualTo("HOSTNAME(42)"));
        }

        [Test]
        public void GIVEN_invalid_address_WHEN_parsing_THE_throws()
        {
            var address = new Address("foo", "blahhh");
            var ex = Assert.Throws<Exception>(()=>address.ToXmsAddress());
            Assert.That(ex.Message, Is.EqualTo("Unable to parse foo@blahhh to xms address."));
        }
    }
}