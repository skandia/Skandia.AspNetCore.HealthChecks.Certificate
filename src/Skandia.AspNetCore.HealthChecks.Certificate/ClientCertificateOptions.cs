using System.Security.Cryptography.X509Certificates;

namespace Skandia.AspNetCore.HealthChecks.Certificate
{
    public class ClientCertificateOptions
    {
        public string Thumbprint;
        public bool MustHavePrivateKey;
        public StoreName StoreName;
        public StoreLocation StoreLocation;
        public bool ValidOnly;
    }
}
