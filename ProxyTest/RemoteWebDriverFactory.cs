using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace ProxyTest
{
    public class RemoteWebDriverFactory : WebDriverFactory
    {
        private static readonly TimeSpan NodeRebootTimeout = TimeSpan.FromMinutes(2);

        private readonly TimeSpan _commandTimeout;

        private readonly String _hubHost;

        private readonly Int32 _hubPort;

        private readonly ILog _logger = LogManager.GetLogger(typeof(RemoteWebDriverFactory));

        private readonly Lazy<IFileDetector> _lazyFileDetector = new Lazy<IFileDetector>(
            () => new LocalFileDetector()
        );

        /// <summary>
        ///     Creates new instance of RemoteWebDriverFactory
        /// </summary>
        /// <param name="browserType">Browser Type</param>
        /// <param name="hubHost">Selenium Hub host</param>
        /// <param name="hubPort">Selenium hub port</param>
        /// <param name="commandTimeout">
        ///     how long WebDriver will wait for remote server response
        /// </param>
        public RemoteWebDriverFactory(BrowserType browserType, String hubHost, Int32 hubPort, TimeSpan commandTimeout) : base(browserType)
        {
            _hubHost = hubHost;
            _hubPort = hubPort;
            _commandTimeout = commandTimeout;
        }

        public void Start_Tunnel(String Host = null, String Port = null)
        {
            var process = new System.Diagnostics.Process();
            //-------------------- Initializing and starting tunnel using binary------------------------------------------
            var startInfo = new System.Diagnostics.ProcessStartInfo();

            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "C://Users//mayankmaurya//Documents//Repo//LT_Windows//LT.exe";
            startInfo.Arguments = "LT  --user userID --key accessKEy";
            if (!String.IsNullOrEmpty(Host))
            {
                startInfo.Arguments = "LT  --user userID --key accessKEy " + "--proxy-host " + Host + " --proxy-port " + Port + " --ingress-only" + " --verbose" + " --mitm";

            }
            process.StartInfo = startInfo;
            //process.StartInfo.UseShellExecute = false;

            process.Start();
            //94.153.200.148
            Thread.Sleep(10000);

        }

        private ICapabilities foo(OpenQA.Selenium.Proxy proxy)
        {
            var hostPort = proxy.HttpProxy.Split(':');

           Start_Tunnel(hostPort[0], hostPort[1]);

            var capabilities = new DesiredCapabilities();
            capabilities.SetCapability("name", "testing");
            //=========================== desktop
            capabilities.SetCapability("platform", "Windows 10"); // Selected Operating System
            capabilities.SetCapability("browser", "Chrome"); // Selected browser name
            capabilities.SetCapability("version", "99");
            capabilities.SetCapability("network", true);

            capabilities.SetCapability("tunnel", true);
            //capabilities.SetCapability("tunnelName", "LambdaTest_uz004");// Selected browser version
            //capabilities.SetCapability("resolution", "1024x768"); // Selected screen resolution
            //capabilities.SetCapability("selenium_version", "3.13.0");
            return capabilities;
        }

        protected override IWebDriver CreateWebDriver(OpenQA.Selenium.Proxy proxy, string culture)
        {

            var hubUri = new Uri($"http://{_hubHost}:{_hubPort}/wd/hub");
            hubUri = new Uri($"https://userID:accessKEy@hub.lambdatest.com/wd/hub");

            var capabilities = foo(proxy);

            for (var i = 0; ; i++)
            {
                RemoteWebDriver wd = null;

                try
                {
                    wd = new RemoteWebDriver(hubUri, capabilities, _commandTimeout)
                    {
                        FileDetector = _lazyFileDetector.Value
                    };

                    wd.Navigate().GoToUrl("about:blank");

                    return wd;
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to create RemoteWebDriver", ex);

                    if (wd != null)
                    {
                        wd.Quit();
                        wd.Dispose();
                    }

                    if (i >= 5)
                    {
                        throw;
                    }

                    if (IsNodeRebootTriggeredError(ex))
                    {
                        _logger.WarnFormat("There are no nodes matching requested capabilities. Waiting for node to reboot for {0}.", NodeRebootTimeout);

                        Thread.Sleep(NodeRebootTimeout);
                    }
                }
            }
        }

        private static bool IsNodeRebootTriggeredError(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }

            return ex.Message.Contains("Empty pool of VM for setup Capabilities")
                   || ex.Message.Contains("Error forwarding the new session")
                   || ex.Message.Contains("Could not find a connected Android device.")
                   || ex.Message.Contains("was not in the list of connected devices");
        }

    }
}