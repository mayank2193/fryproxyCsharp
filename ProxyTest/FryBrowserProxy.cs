using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FryProxy;

namespace ProxyTest
{
    public class FryBrowserProxy : IBrowserProxy
    {

        private readonly HttpProxyServer _httpProxyServer;

        private readonly IPEndPoint _proxyHost;

        private readonly HttpProxyServer _sslProxyServer;

        public FryBrowserProxy(X509Certificate certificate, BrowserProxyConfigurationSection settings)
        {
            var ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(
                address => address.AddressFamily == AddressFamily.InterNetwork);

            _proxyHost = new IPEndPoint(ipAddress, 0);

            _httpProxyServer = new HttpProxyServer(_proxyHost, new HttpProxy
            {
                ClientReadTimeout = settings.ClientReadTimeout,
                ClientWriteTimeout = settings.ClientWriteTimeout,
                ServerReadTimeout = settings.ServerReadTimeout,
                ServerWriteTimeout = settings.ServerWriteTimeout,
                OnRequestReceived = HandleRequestReceived,
                OnResponseReceived = HandleResponseReceived,
                OnProcessingComplete = HandleProcessingComplete,
                OnServerConnected = HandleServerConnected,
                OnResponseSent = HandleResponseSent
            });

            _sslProxyServer = new HttpProxyServer(_proxyHost, new SslProxy(certificate)
            {
                ClientReadTimeout = settings.ClientReadTimeout,
                ClientWriteTimeout = settings.ClientWriteTimeout,
                ServerReadTimeout = settings.ServerReadTimeout,
                ServerWriteTimeout = settings.ServerWriteTimeout,
                OnRequestReceived = HandleRequestReceived,
                OnResponseReceived = HandleResponseReceived,
                OnProcessingComplete = HandleProcessingComplete,
                OnServerConnected = HandleServerConnected,
                OnResponseSent = HandleResponseSent

            });
        }

        public Action<ProcessingContext> OnRequestReceived { get; set; }
        public Action<ProcessingContext> OnResponseReceived { get; set; }
        public Action<ProcessingContext> OnProcessingComplete { get; set; }
        public Action<ProcessingContext> OnServerConnected { get; set; }
        public Action<ProcessingContext> OnResponseSent { get; set; }

        public Boolean IsListening
        {
            get { return _httpProxyServer.IsListening && _sslProxyServer.IsListening; }
        }

        public Int32 HttpPort
        {
            get { return _httpProxyServer.ProxyEndPoint.Port; }
        }

        public Int32 SslPort
        {
            get { return _sslProxyServer.ProxyEndPoint.Port; }
        }

        public String HostName
        {
            get { return _proxyHost.Address.ToString(); }
        }

        public void Start()
        {
            WaitHandle.WaitAll(new[] {
                _httpProxyServer.Start(),
                _sslProxyServer.Start()
            });
        }

        public void Stop()
        {
            _httpProxyServer.Stop();
            _sslProxyServer.Stop();
        }

        private void HandleResponseReceived(ProcessingContext context)
        {
            if (OnResponseReceived == null)
            {
                return;
            }

            OnResponseReceived(context);
        }

        private void HandleRequestReceived(ProcessingContext context)
        {
            if (OnRequestReceived == null)
            {
                return;
            }

            OnRequestReceived(context);
        }

        private void HandleServerConnected(ProcessingContext context)
        {
            if (OnServerConnected == null)
            {
                return;
            }

            OnServerConnected(context);
        }

        private void HandleResponseSent(ProcessingContext context)
        {
            if (OnResponseSent == null)
            {
                return;
            }

            OnResponseSent(context);
        }



        private void HandleProcessingComplete(ProcessingContext context)
        {
            if (OnProcessingComplete == null)
            {
                return;
            }

            OnProcessingComplete(context);
        }
    }
}