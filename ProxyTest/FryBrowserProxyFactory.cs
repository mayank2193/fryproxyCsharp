using System;
using System.Security.Cryptography.X509Certificates;

namespace ProxyTest
{
    public class FryBrowserProxyFactory : IBrowserProxyFactory
    {

        private const String CertificatePassword = "fry";

        private const String CertificateFileName = "fry.pfx";

        private readonly Lazy<X509Certificate> _lazyCertificate;

        private readonly BrowserProxyConfigurationSection _settings;

        public FryBrowserProxyFactory(BrowserProxyConfigurationSection settings)
        {
            _lazyCertificate = new Lazy<X509Certificate>(CreateCertificate);
            _settings = settings;
            _absoluteAssemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        private string _absoluteAssemblyPath;

        public String AbsoluteAssemblyPath => AppDomain.CurrentDomain.BaseDirectory;

        private X509Certificate2 CreateCertificate()
        {
            return new X509Certificate2(String.Format(@"{0}\{1}", _absoluteAssemblyPath ?? AbsoluteAssemblyPath, CertificateFileName), CertificatePassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        }

        public IBrowserProxy Create()
        {
            return new FryBrowserProxy(_lazyCertificate.Value, _settings);
        }

    }
}