using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;

namespace ProxyTest
{
    public abstract class WebDriverFactory : IWebDriverFactory
    {
        protected WebDriverFactory(BrowserType browserType)
        {
            BrowserType = browserType;
        }

        public BrowserType BrowserType { get; set; }

        public EventFiringWebDriver Create()
        {
            return Create(null);
        }

        public EventFiringWebDriver Create(OpenQA.Selenium.Proxy proxy, string culture = null)
        {
            var driver = CreateWebDriver(proxy, culture);
            return driver is EventFiringWebDriver
                ? (EventFiringWebDriver)driver
                : new EventFiringWebDriver(driver);
        }

        protected abstract IWebDriver CreateWebDriver(OpenQA.Selenium.Proxy proxy, string culture = null);

    }
}