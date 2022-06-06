using OpenQA.Selenium.Support.Events;

namespace ProxyTest
{
    public class LazyBrowser : AbstractBrowser
    {
        protected readonly IWebDriverFactory DriverFactory;

        private EventFiringWebDriver _currentDriver;

        public LazyBrowser(IWebDriverFactory driverFactory)
        {
            DriverFactory = driverFactory;
        }

        /// <summary>
        ///     Managed WebDriver instance
        /// </summary>
        public override EventFiringWebDriver WebDriver
        {
            get { return _currentDriver ?? (_currentDriver = DriverFactory.Create()); }
        }

        public override bool IsOpened
        {
            get { return _currentDriver != null; }
        }

        public override void Dispose()
        {
            if (IsOpened)
            {
                _currentDriver.Dispose();
                _currentDriver = null;
            }
        }
    }
}