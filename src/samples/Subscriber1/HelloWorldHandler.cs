using System;
using Messages;
using NServiceBus;
using log4net;

namespace Subscriber1
{
    public class HelloWorldHandler : IHandleMessages<HelloWorld>
    {
        private static int counter;
        private readonly IBus bus;
        private static readonly ILog log = LogManager.GetLogger(typeof (HelloWorldHandler));

        public HelloWorldHandler(IBus bus)
        {
            this.bus = bus;
        }

        public void Handle(HelloWorld message)
        {
            System.Threading.Interlocked.Increment(ref counter);
            log.Info(counter + " HelloWorld");
        }
    }
}