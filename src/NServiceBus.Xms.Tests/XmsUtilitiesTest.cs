using System;
using System.Collections.Generic;
using NServiceBus.Unicast.Transport;
using NUnit.Framework;

namespace NServiceBus.Xms.Tests
{
    [TestFixture]
    public class XmsUtilitiesTest
    {
        Address address = TestTargets.InputQueue.ToNsbAddress();

        [SetUp]
        public void SetUp()
        {
            XmsUtilities.Purge(address.ToXmsAddress());
        }

        [Test]
        public void GIVEN_transaport_message_WHEN_sent_over_the_wire_THEN_preserves_ReplyToAddress()
        {
            TestMessageProperty2(expected => expected.ReplyToAddress = address, actual => actual.ReplyToAddress);
        }

        [Test]
        public void GIVEN_transaport_message_WHEN_sent_over_the_wire_THEN_preserves_MessageIntent()
        {
            TestMessageProperty2(expected => expected.MessageIntent = MessageIntentEnum.Send, actual => actual.MessageIntent);
        }

        [Test]
        public void GIVEN_transaport_message_WHEN_sent_over_the_wire_THEN_Recoverable_false_is_always_true()
        {
            TestMessageProperty2(expected => expected.Recoverable = false, expected => expected.Recoverable, actual => !actual.Recoverable);
        }

        [Test]
        public void GIVEN_transaport_message_WHEN_sent_over_the_wire_THEN_preserves_Message_header_with_value()
        {
            TestMessageProperty2(expected =>
                {
                    expected.Headers = new Dictionary<string, string>();
                    expected.Headers["foo"] = "boo";
                }, actual => actual.Headers["foo"]);
        }

        [Test]
        public void GIVEN_transaport_message_WHEN_sent_over_the_wire_THEN_preserves_Message_header_with_non_standard_key()
        {
            TestMessageProperty2(expected =>
            {
                expected.Headers = new Dictionary<string, string>();
                expected.Headers["foo.dot"] = "foo.dot";
            }, actual => actual.Headers["foo.dot"]);
        }

        private void TestMessageProperty2<T>(Action<TransportMessage> assign, Func<TransportMessage, T> getter)
        {
            TestMessageProperty2<T>(assign, getter, getter);
        }

        private void TestMessageProperty2<T>(Action<TransportMessage> assign, Func<TransportMessage, T> expectedGetter, Func<TransportMessage, T> actualGetter)
        {
            var expected = new TransportMessage();
            TransportMessage actual;

            assign(expected);

            using (var producer = new XmsProducer(address.ToXmsAddress(), false))
            {
                var bytesMessage = producer.CreateBytesMessage();
                XmsUtilities.Convert(expected, bytesMessage);
                producer.Send(bytesMessage);
            }

            using (var consumer = new XmsConsumer(address.ToXmsAddress(), false))
            {
                var bytesMessage = consumer.ReceiveNoWait();
                actual = XmsUtilities.Convert(bytesMessage);
            }

            Assert.That(actualGetter(actual), Is.EqualTo(expectedGetter(expected)));
        }
    }
}