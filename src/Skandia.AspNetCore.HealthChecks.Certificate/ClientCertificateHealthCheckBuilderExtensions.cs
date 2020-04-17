using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Skandia.AspNetCore.HealthChecks.Certificate
{
    public static class ClientCertificateHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddClientCertificateCheck(
            this IHealthChecksBuilder builder,
            string name,
            string thumbprint,
            bool mustHavePrivateKey = false,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.LocalMachine,
            bool validOnly = true,
            HealthStatus? failureStatus = HealthStatus.Unhealthy,
            IEnumerable<string> tags = null,
            bool treatEmptyThumbprintAsOk = true
            )
        {
            // Register a check of type GCInfo
            builder.AddCheck<ClientCertificateHealthCheck>(name, failureStatus ?? HealthStatus.Unhealthy, tags);

            // Configure named options to pass the threshold into the check.
            builder.Services.Configure<ClientCertificateOptions>(name, options =>
                {
                    options.MustHavePrivateKey = mustHavePrivateKey;
                    options.Thumbprint = thumbprint;
                    options.StoreName = storeName;
                    options.StoreLocation = storeLocation;
                    options.ValidOnly = validOnly;
                    options.TreatEmptyThumbprintAsOk = treatEmptyThumbprintAsOk;
                });
            

            return builder;
        }
    }
}
