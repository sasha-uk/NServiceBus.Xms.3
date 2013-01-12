
namespace NServiceBus.Xms
{
    public static class ConfigureXmsMessageQueue
    {
        public static bool Selected { get; set; }


        public static Configure XmsTransport(this Configure config)
        {
            Selected = true;

            config.Configurer.ConfigureComponent<XmsReceiveMessages>(DependencyLifecycle.SingleInstance)
                  .ConfigureProperty(p => p.PurgeOnStartup, ConfigurePurging.PurgeRequested);

            config.Configurer.ConfigureComponent<XmsSendMessages>(DependencyLifecycle.SingleInstance);

            var cfg = Configure.GetConfigSection<XmsMessageQueueConfig>();

            var useJournalQueue = false;
            var useDeadLetterQueue = true;

            if (cfg != null)
            {
                useJournalQueue = cfg.UseJournalQueue;
                useDeadLetterQueue = cfg.UseDeadLetterQueue;
            }
            config.Configurer.ConfigureProperty<XmsSendMessages>(t => t.UseDeadLetterQueue, useDeadLetterQueue);
            config.Configurer.ConfigureProperty<XmsSendMessages>(t => t.UseJournalQueue, useJournalQueue);

            return config;
        }
    }
}