
namespace NServiceBus.Xms
{
    public static class ConfigureXmsMessageQueue
    {
        public static bool Selected { get; set; }


        public static Configure XmsTransport(this Configure config)
        {
            Selected = true;

            config.Configurer.ConfigureComponent<XmsMessageReceiver>(DependencyLifecycle.SingleInstance)
                  .ConfigureProperty(p => p.PurgeOnStartup, ConfigurePurging.PurgeRequested);

            config.Configurer.ConfigureComponent(() => new XmsMessageSender(Unicast.Transport.Transactional.Config.Bootstrapper.IsTransactional),DependencyLifecycle.SingleInstance);

            var cfg = Configure.GetConfigSection<XmsMessageQueueConfig>();

            var useJournalQueue = false;
            var useDeadLetterQueue = true;

            if (cfg != null)
            {
                useJournalQueue = cfg.UseJournalQueue;
                useDeadLetterQueue = cfg.UseDeadLetterQueue;
            }
            config.Configurer.ConfigureProperty<XmsMessageSender>(t => t.UseDeadLetterQueue, useDeadLetterQueue);
            config.Configurer.ConfigureProperty<XmsMessageSender>(t => t.UseJournalQueue, useJournalQueue);

            return config;
        }
    }
}