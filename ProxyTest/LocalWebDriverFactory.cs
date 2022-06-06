using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ProxyTest
{
        public class LocalWebDriverFactory : WebDriverFactory
        {

            private readonly TimeSpan _commandTimeout;

            /// <summary>
            ///     Create new instance
            /// </summary>
            /// <param name="browserType">
            ///     which browsers will factory create
            /// </param>
            /// <param name="commandTimeout">
            ///     How long WebDriver will wait for remote server response
            /// </param>
            public LocalWebDriverFactory(BrowserType browserType, TimeSpan commandTimeout) : base(browserType)
            {
                _commandTimeout = commandTimeout;
            }

            protected override IWebDriver CreateWebDriver(OpenQA.Selenium.Proxy proxy, string culture = null)
            {
                switch (BrowserType)
                {
                    case BrowserType.Chrome:
                        return new ChromeDriver(ChromeDriverService.CreateDefaultService(), DriverOptions.ForChrome(proxy, culture), _commandTimeout);
                }

                throw new ArgumentException(String.Format("Unsupported browser type: {0}", BrowserType));
            }

        }
}