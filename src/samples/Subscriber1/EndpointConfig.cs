using Autofac;
using Messages;
using log4net.Config;

namespace Subscriber1 
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
            IContainer container = builder.Build();

            Configure.With()
                .AutofacBuilder(container)
                .XmlSerializer(Namespaces.Default)
                .MsmqTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)
                /*.XmsTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)*/
                .UnicastBus()
                    .LoadMessageHandlers()
                .CreateBus();
        }
    }
}