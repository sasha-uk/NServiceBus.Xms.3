using System.Configuration;

namespace NServiceBus.Xms
{
    public class XmsMessageQueueConfig : ConfigurationSection
    {
        [ConfigurationProperty("UseDeadLetterQueue", DefaultValue = true, IsRequired = false)]
        public bool UseDeadLetterQueue
        {
            get
            {
                return (bool) this["UseDeadLetterQueue"];
            }
            set
            {
                this["UseDeadLetterQueue"] = value;
                //this["UseDeadLetterQueue"] = (object) (bool) (value ? 1 : 0);
            }
        }

        [ConfigurationProperty("UseJournalQueue", IsRequired = false)]
        public bool UseJournalQueue
        {
            get
            {
                return (bool) this["UseJournalQueue"];
            }
            set
            {
                //this["UseJournalQueue"] = (object) (bool) (value ? 1 : 0);
                this["UseJournalQueue"] = value;
            }
        }
    }
}