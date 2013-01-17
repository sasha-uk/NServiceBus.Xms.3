using System;
using Messages;
using NServiceBus;

namespace Subscriber1
{
    public class HelloWorldHandler : IHandleMessages<HelloWorld>
    {
        private static int counter;
        private readonly IBus bus;

        public HelloWorldHandler(IBus bus)
        {
            this.bus = bus;
        }

        public void Handle(HelloWorld message)
        {
            System.Threading.Interlocked.Increment(ref counter);
            Console.WriteLine(counter + " HelloWorld:" + DateTime.UtcNow);
        }
    }
}