using System;
using OpenQA.Selenium.Support.Events;

namespace ProxyTest
{
    public class ProxiedWebDriverFactory : IWebDriverFactory
    {

        private readonly IWebDriverFactory _driverFactory;

        private readonly IBrowserProxy _proxy;

        public ProxiedWebDriverFactory(IWebDriverFactory driverFactory, IBrowserProxy proxy)
        {
            _driverFactory = driverFactory;
            _proxy = proxy;
        }

        public BrowserType BrowserType
        {
            get { return _driverFactory.BrowserType; }
            set { _driverFactory.BrowserType = value; }
        }

        public EventFiringWebDriver Create()
        {
            _proxy.Start();

            return Create(new OpenQA.Selenium.Proxy
            {
                HttpProxy = String.Format("{0}:{1}", _proxy.HostName, _proxy.HttpPort),
                SslProxy = String.Format("{0}:{1}", _proxy.HostName, _proxy.SslPort)
            });
        }

        public EventFiringWebDriver Create(OpenQA.Selenium.Proxy proxy, string culture = null)
        {
            return _driverFactory.Create(proxy, culture);
        }

    }
}