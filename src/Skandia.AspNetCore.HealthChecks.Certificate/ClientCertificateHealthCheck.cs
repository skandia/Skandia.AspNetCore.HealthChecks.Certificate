using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Skandia.AspNetCore.HealthChecks.Certificate
{
    public class ClientCertificateHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<ClientCertificateOptions> _options;

        public ClientCertificateHealthCheck(IOptionsMonitor<ClientCertificateOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var options = _options.Get(context.Registration.Name);

            return await ClientCertificateHelper.ValidateCertificate(options, context.Registration.FailureStatus);
        }
    }
}
