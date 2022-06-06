using System.Collections.Generic;
using System.Threading;
using FryProxy;

namespace ProxyTest
{
    public class ProxiedBrowser : LazyBrowser
    {

        private readonly IBrowserProxy _proxy;

        public ProxiedBrowser(IWebDriverFactory driverFactory, IBrowserProxy proxy) : base(driverFactory)
        {
            _proxy = proxy;
        }

        public IBrowserProxy Proxy
        {
            get { return _proxy; }
        }

        public override void Dispose()
        {
            base.Dispose();
            Proxy.Stop();
        }
    }
}