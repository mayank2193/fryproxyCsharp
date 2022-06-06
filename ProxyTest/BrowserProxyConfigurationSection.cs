using System;

namespace ProxyTest
{
    public class BrowserProxyConfigurationSection
    {
        public TimeSpan ClientReadTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public TimeSpan ClientWriteTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public TimeSpan ServerWriteTimeout { get; set; } = TimeSpan.FromSeconds(20);

        public TimeSpan ServerReadTimeout { get; set; } = TimeSpan.FromSeconds(20);
    }
}