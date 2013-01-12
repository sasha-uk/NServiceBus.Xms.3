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
        public void Init()
        {
            SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);
            var builder = new ContainerBuilder();
            builder.RegisterType<InMemorySubscriptionStorage>().As<ISubscriptionStorage>();
            IContainer container = builder.Build();

            Configure
                .With()
                .AutofacBuilder(container)
                .XmlSerializer(Namespaces.Default)
                .XmsTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)
                //.DBSubcriptionStorage()
                /*.MsmqTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)*/
                .UnicastBus()
                    .ImpersonateSender(false)
                .CreateBus();
        }
    }
}