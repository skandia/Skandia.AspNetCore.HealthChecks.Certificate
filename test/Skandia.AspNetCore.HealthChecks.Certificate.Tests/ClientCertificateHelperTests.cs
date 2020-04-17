using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Skandia.AspNetCore.HealthChecks.Certificate.Tests
{
    public class ClientCertificateHelperTests
    {
        [Fact]
        public async void ClientCertificateHelper_ValidateCertificate_ExistingThumbprint_ReturnsOk()
        {
            // Arrange
            // Fetch a thumbprint from a cert hopefully already in store... could we assume it exists one? Or must insert cert in test case?
            var thumbprint = GetThumbprint(StoreName.My, StoreLocation.LocalMachine, false, true);
            var anExistingCertFound = thumbprint != null;
            Assert.True(anExistingCertFound,
                "No existing suitable cert found in store for this test (add or rewrite test needed...)");
            var options = new ClientCertificateOptions
            {
                Thumbprint = thumbprint,
                StoreLocation = StoreLocation.LocalMachine,
                StoreName = StoreName.My,
                MustHavePrivateKey = false,
                ValidOnly = true
            };

            // Act
            var result = await ClientCertificateHelper.ValidateCertificate(options, HealthStatus.Unhealthy);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Contains("Certificate OK ", result.Description);
        }

        [Fact]
        public async void ClientCertificateHelper_ValidateCertificate_ExistingThumbprintWithPrivateKey_ReturnsOk()
        {
            // Arrange
            // Fetch a thumbprint from a cert hopefully already in store... could we assume it exists one? Or must insert cert in test case?
            var thumbprint = GetThumbprint(StoreName.My, StoreLocation.LocalMachine, true, true);
            var anExistingCertFound = thumbprint != null;
            Assert.True(anExistingCertFound,
                "No existing suitable cert found in store for this test (add or rewrite test needed...)");

            var options = new ClientCertificateOptions
            {
                Thumbprint = thumbprint,
                StoreLocation = StoreLocation.LocalMachine,
                StoreName = StoreName.My,
                MustHavePrivateKey = true,
                ValidOnly = true
            };

            // Act
            var result = await ClientCertificateHelper.ValidateCertificate(options, HealthStatus.Unhealthy);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Contains("Certificate OK ", result.Description);
        }

        [Fact]
        public async void ClientCertificateHelper_ValidateCertificate_NonExistingThumbprint_ReturnsFail()
        {
            // Arrange
            var thumbprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var options = new ClientCertificateOptions
            {
                Thumbprint = thumbprint,
                StoreLocation = StoreLocation.LocalMachine,
                StoreName = StoreName.My,
                MustHavePrivateKey = true,
                ValidOnly = true
            };

            // Act
            var result = await ClientCertificateHelper.ValidateCertificate(options, HealthStatus.Unhealthy);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains(" was not found in", result.Description);
        }

        [Fact]
        public async void ClientCertificateHelper_ValidateCertificate_CertificateWithoutPrivateKey_WhenRequirePrivateKey_ReturnsFail()
        {
            // Arrange
            // Fetch a thumbprint from a cert hopefully already in store... could we assume it exists one? Or must insert cert in test case?
            var thumbprint = GetThumbprint(StoreName.My, StoreLocation.LocalMachine, false, true);
            var anExistingCertFound = thumbprint != null;
            Assert.True(anExistingCertFound,
                "No existing suitable cert found in store for this test (add or rewrite test needed...)");

            var options = new ClientCertificateOptions
            {
                Thumbprint = thumbprint,
                StoreLocation = StoreLocation.LocalMachine,
                StoreName = StoreName.My,
                MustHavePrivateKey = true,
                ValidOnly = true
            };

            // Act
            var result = await ClientCertificateHelper.ValidateCertificate(options, HealthStatus.Unhealthy);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Contains("has no corresponding private key", result.Description);
        }

        public static string GetThumbprint(StoreName storeName, StoreLocation storeLocation, bool mustHavePrivateKey,
            bool validOnly)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                foreach (var certificate in store.Certificates)
                {
                    if (mustHavePrivateKey && !certificate.HasPrivateKey)
                    {
                        continue;
                    }

                   
                    if (!mustHavePrivateKey && certificate.HasPrivateKey)
                    { 
                        continue;
                    }

                    if (validOnly && !certificate.Verify())
                    {
                        continue;
                    }

                    if (certificate.IssuerName.Name == "CN=localhost")
                    {
                        continue;
                    }

                    return certificate.Thumbprint;
                }

                return null;
            }
        }
    }
}
