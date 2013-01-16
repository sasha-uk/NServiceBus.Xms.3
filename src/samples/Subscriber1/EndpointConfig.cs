using System.Configuration;
using Autofac;
using Messages;
using NServiceBus.Xms;
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
            //SetLoggingLibrary.Log4Net(XmlConfigurator.Configure);

            var builder = new ContainerBuilder();
            var container = builder.Build();

            Configure
                .With()
                .DefineEndpointName(() => ConfigurationManager.AppSettings["inputQueueName"])
                .AutofacBuilder(container)
                .DisableSecondLevelRetries() // TODO:
                .XmlSerializer(Namespaces.Default)
                .XmsTransport()
                    .IsTransactional(true)
                    .PurgeOnStartup(false)
                .UnicastBus()
                    .LoadMessageHandlers()
                .CreateBus();
        }
    }
}