using System;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace ProxyTest
{
    public abstract class AbstractBrowser : IDisposable
    {
        protected TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(1);

        protected TimeSpan DefaultCommandTimeout = TimeSpan.FromSeconds(10);

        protected AbstractBrowser(AbstractBrowser browser)
        {
            if (browser == null)
            {
                throw new ArgumentNullException(nameof(browser));
            }

            Logger = LogManager.GetLogger(GetType());
        }

        protected AbstractBrowser()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        public ILog Logger { get; private set; }

        public string CurrentUrl => WebDriver.Url;

        /// <summary>
        ///     Indicate current browser state
        /// </summary>
        public abstract Boolean IsOpened { get; }

        /// <summary>
        ///     Managed IWebDriver instance
        /// </summary>
        public abstract EventFiringWebDriver WebDriver { get; }

        /// <summary>
        ///     Close managed IWebDriver instance
        /// </summary>
        public abstract void Dispose();


        /// <summary> 
        ///     Refresh current page
        /// </summary>
        public void Refresh()
        {
            WebDriver.Navigate().Refresh();
        }

        /// <summary>
        ///     Safely delete all cookies
        /// </summary> 
        public void ClearCookies()
        {
            try
            {
                WebDriver.Manage().Cookies.DeleteAllCookies();
            }
            catch (WebDriverException ex)
            {
                Logger.Warn("failed to clear browser cookies", ex);
            }
        }






        /// <summary>
        ///     Navigates to given URL
        /// </summary>
        /// <param name="url">URL to open</param>
        public void NavigateTo(String url)
        {
            WebDriver.Navigate().GoToUrl(url);
        }

        /// <summary> Resolve URL for given page and navigate to it </summary>
        /// <typeparam name="TPage">page type</typeparam>
        /// <param name="args">URL path arguments</param>
        public void NavigateTo<TPage>(params Object[] args) where TPage : class
        {
            NavigateTo<TPage>(String.Empty, args);
        }

        /// <summary>
        /// Navigates forward given number of times, it automatically accepts exit message if present
        /// </summary>
        /// <param name="times">Number of times for navigation forward</param>
        public void NavigateForward()
        {
            try
            {
                WebDriver.Navigate().Forward();
            }
            catch (InvalidOperationException e)
            {
                Logger.Warn("failed to navigate forward in the browser", e);
                WebDriver.SwitchTo().Alert().Accept();
            }
        }

        /// <summary>
        ///     Navigate to browser internal blank page
        /// </summary>
        public void OpenBlankPage()
        {
            NavigateTo("about:blank");
        }


        public void CloseAllTabsExceptCurrent()
        {
            String OriginalHandle = WebDriver.CurrentWindowHandle;

            //Do something to open new tabs

            foreach (String handle in WebDriver.WindowHandles)
            {
                if (!handle.Equals(OriginalHandle))
                {
                    WebDriver.SwitchTo().Window(handle);
                    WebDriver.Close();
                }
            }

            WebDriver.SwitchTo().Window(OriginalHandle);
        }

        /// <summary>
        ///     Accept native browser's pop up
        /// </summary>
        public void AcceptNativeBrowserPopUp()
        {
            WebDriverWait wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds(30));
            wait.Until(IsAlertShown);
            WebDriver.SwitchTo().Alert().Accept();
        }

        private bool IsAlertShown(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
            }
            catch (NoAlertPresentException e)
            {
                return false;
            }
            return true;
        }

        public ScreenOrientation ScreenOrientation
        {
            get
            {
                return RotableDriver.Orientation;
            }
            set
            {
                RotableDriver.Orientation = value;

            }
        }
        private IRotatable RotableDriver
        {
            get
            {
                var rotatable = WebDriver.WrappedDriver as IRotatable;

                if (rotatable == null)
                {
                    throw new InvalidOperationException(
                        $"Underlying driver does not support screen orientation change: {WebDriver.WrappedDriver.GetType()}"
                    );
                }

                return rotatable;
            }
        }

        public string GetLocalStorageValue(string key)
        {
            return (string)WebDriver.ExecuteScript($"return localStorage.getItem('{key}')", "");
        }

        public string SetLocalStorageValue(string key, string value)
        {
            return (string)WebDriver.ExecuteScript($"localStorage.setItem('{key}','{value}')", "");
        }
    }
}