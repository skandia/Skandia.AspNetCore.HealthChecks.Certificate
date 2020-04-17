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
            ClientCertificateOptions options,
            HealthStatus failureStatus)
        {
            if (string.IsNullOrEmpty(options.Thumbprint) && options.TreatEmptyThumbprintAsOk)
            {
                return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, description: "Certificate OK since thumbprint is empty and that is allowed"));
            }
            //creates the store based on the input name and location e.g. name=My
            var certStore = new X509Store(options.StoreName, options.StoreLocation);
            //finds the certificate in this store
            certStore.Open(OpenFlags.ReadOnly);
            var certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, options.Thumbprint, options.ValidOnly);
            certStore.Dispose();
            if (certCollection.Count == 0)
            {
                return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"The certificate with thumbprint '{options.Thumbprint}' was not found in {options.StoreName}:{options.StoreLocation}"));
            }

            if (options.MustHavePrivateKey)
            {
                if (!certCollection[0].HasPrivateKey)
                {
                    return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"The certificate with thumbprint '{options.Thumbprint}' in {options.StoreName}:{options.StoreLocation} has no corresponding private key"));
                }
                try
                {
                    // ReSharper disable once UnusedVariable
                    var privKey = certCollection[0].GetRSAPrivateKey(); //NOTE: Check you have access to private key
                }
                catch
                {
                    return await Task.FromResult(new HealthCheckResult(failureStatus, description: $"No permission to read Private key with thumbprint '{options.Thumbprint}'"));
                }

            }
            return await Task.FromResult(new HealthCheckResult(HealthStatus.Healthy, description: $"Certificate OK (valid required: {options.ValidOnly}) with thumbprint '{options.Thumbprint}' found in {options.StoreName}:{options.StoreLocation}. Private key: {options.MustHavePrivateKey}"));
        }
    }
}
