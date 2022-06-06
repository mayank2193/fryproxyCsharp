using OpenQA.Selenium.Support.Events;

namespace ProxyTest
{
    /// <summary>
    ///     Creates instances of EventFiringWebDriver
    /// </summary>
    public interface IWebDriverFactory
    {
        /// <summary>
        ///     Type of browser which created WebDriver will be connected to
        /// </summary>
        BrowserType BrowserType { get; set; }

        /// <summary>
        ///     Create EventFiringWebDriver
        /// </summary>
        /// <returns>new WebDriver instance</returns>
        EventFiringWebDriver Create();

        /// <summary>
        ///     Create EventFiringWebDriver using provided proxy
        /// </summary>
        /// <param name="proxy">proxy which created webdriver should use</param>
        /// <param name="culture">culture set as default to profile like  "fr-FR" </param>
        /// <returns>new WebDriver instance</returns>
        EventFiringWebDriver Create(OpenQA.Selenium.Proxy proxy, string culture);
    }
}