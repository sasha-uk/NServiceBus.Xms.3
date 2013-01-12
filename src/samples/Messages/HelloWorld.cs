using System;
using NServiceBus;

namespace Messages 
{
    public class HelloWorld : IEvent
    {
        public DateTime Date { get; set; }
    }
}