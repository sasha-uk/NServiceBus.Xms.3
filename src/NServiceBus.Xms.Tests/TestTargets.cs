using System.Configuration;

namespace NServiceBus.Xms.Tests
{
    public static class TestTargets
    {
        public static XmsAddress InputQueue
        {
            get { return ConfigurationManager.AppSettings["target"].ToXmsAddress(); }
        }

        public static XmsAddress Error
        {
            get { return ConfigurationManager.AppSettings["error"].ToXmsAddress(); }
        }
    }
}