using System;

namespace NServiceBus.Xms.Utils
{
    public static class IntExtensions
    {
        public static TimeSpan Seconds(this int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        public static TimeSpan Milliseconds(this int milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}