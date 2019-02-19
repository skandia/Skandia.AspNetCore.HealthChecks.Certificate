using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Skandia.AspNetCore.HealthChecks.Certificate
{
    public static class ClientCertificateHelper
    {
        public static async Task<HealthCheckResult> ValidateCertificate(
            string thumbprint,
            StoreName storeName,
            StoreLocation location,
            bool mustHavePrivateKey,
            bool validOnly,
            HealthStatus failureStatus)
        {
            //creates the store based on the input name and location e.g. name=My
                var certStore = new X509Store(storeName, location);
                //finds the certificate in this store
                certStore.Open(OpenFlags.ReadOnly);
                var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly);
                certStore.Dispose();
                if (certCollection.Count == 0)
                {
                    return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"The certificate with thumbprint '{thumbprint}' was not found in {storeName}:{location}"));
                }

                if (mustHavePrivateKey)
                {
                    if (!certCollection[0].HasPrivateKey)
                    {
                        return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"The certificate with thumbprint '{thumbprint}' in {storeName}:{location} has no corresponding private key"));
                    }
                    try
                    {
                        // ReSharper disable once UnusedVariable
                        var privKey = certCollection[0].GetRSAPrivateKey(); //NOTE: Check you have access to private key
                    }
                    catch
                    {
                        return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"No permission to read Private key with thumbprint '{thumbprint}'"));
                    }

                }
            return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, description: $"Certificate OK (valid required: {validOnly}) with thumbprint '{thumbprint}' found in {storeName}:{location}. Private key: {mustHavePrivateKey}"));
        }
    }
}
