using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ProxyTest
{
    public class Test1
    {

        private ProxiedBrowser proxiedBrowser;
        private AbstractBrowser browser;
        [Test]
        public void ShouldTestProxy()
        {
            var browserConfiguration = new BrowserProxyConfigurationSection();
            var browserFactory = new FryBrowserProxyFactory(browserConfiguration);

            //******************* Use This webdriverFactory to engage LT hub
            var webdriverFactory =new RemoteWebDriverFactory(BrowserType.Chrome, "foo", 111, TimeSpan.FromSeconds(30));

            //This webDriverFactory for testing proxy workability locally. Comment it to work with LT hub.
            //var webdriverFactory = new LocalWebDriverFactory(BrowserType.Chrome, TimeSpan.FromSeconds(30));
           
            var proxy = browserFactory.Create();

            var proxiedBrowserFactory = new ProxiedWebDriverFactory(webdriverFactory, proxy);

            proxiedBrowser = new ProxiedBrowser(proxiedBrowserFactory, proxy);
            var requestCollector = new RequestCollector();

            requestCollector.Attach(proxiedBrowser);
            proxiedBrowser.NavigateTo("about:blank");

            var port = proxiedBrowser.Proxy.HttpPort;

            requestCollector.Attach(proxiedBrowser);

            proxiedBrowser.NavigateTo("https://www.justanswer.com/car/jf001-2014-sprinter-van-will-crank-few-seconds.html?membershipModelTrialType=NoTrial");

            Assert.That(() => requestCollector.Requests.Count, Is.GreaterThan(100).After(1000));

        }

        [TearDown]
        public void TearDown()
        {
            proxiedBrowser.Dispose();
        }

        /*[Test]
        public void ShouldTestBrowser()
        {
            var webdriverFactory = new LocalWebDriverFactory(BrowserType.Chrome, TimeSpan.FromSeconds(30));
            var browser = webdriverFactory.Create(null);
           
        }*/
    }
}
