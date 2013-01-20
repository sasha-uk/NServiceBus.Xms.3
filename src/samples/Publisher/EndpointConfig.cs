using System.Configuration;
using Autofac;
using Messages;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.InMemory;
using NServiceBus.Xms;
using log4net.Config;

namespace Publisher 
{
    using NServiceBus;

	/*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://nservicebus.com/GenericHost.aspx
	*/
    public class EndpointConfig : IConfigureThisEndpoint, IWantCustomInitialization
    {
        public static readonly bool Transactional = true;

        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<InMemorySubscriptionStorage>().As<ISubscriptionStorage>();
            var container = builder.Build();
            
            Configure
                .With()
                .DefineEndpointName(() => ConfigurationManager.AppSettings["inputQueueName"])
                .AutofacBuilder(container)
                .DisableSecondLevelRetries() // TODO:
                .XmlSerializer(Namespaces.Default)
                .InMemorySubscriptionStorage()
                .XmsTransport()
                    .IsTransactional(Transactional)
                    .PurgeOnStartup(false)
                .UnicastBus()
                    .ImpersonateSender(false)
                .CreateBus();
        }
    }
}