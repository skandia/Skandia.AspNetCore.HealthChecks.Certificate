using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Skandia.AspNetCore.HealthChecks.Certificate.Tests
{
    public class ClientCertificateHealthCheckBuilderExtensionsTest
    {
        [Fact]
        public async Task StatusIsHealthyIfCertificateExists()
        {
            // Fetch a thumbprint from a cert hopefully already in store... could we assume it exists one? Or must insert cert in test case?
            var thumbprint = ClientCertificateHelperTests.GetThumbprint(StoreName.My, StoreLocation.LocalMachine, false, true);
            var anExistingCertFound = thumbprint != null;
            Assert.True(anExistingCertFound,
                "No existing suitable cert found in store for this test (add or rewrite test needed...)");

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseHealthChecks("/health");
                    
                })
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddClientCertificateCheck("Cert", thumbprint);

                    services.AddSingleton<IHealthCheck, ClientCertificateHealthCheck>();
                });
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.ToString());
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task StatusIsUnhealthyIfCertificateNotExists()
        {

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseHealthChecks("/health");

                })
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddClientCertificateCheck("Cert", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

                    services.AddSingleton<IHealthCheck, ClientCertificateHealthCheck>();
                });
            var server = new TestServer(builder);
            var client = server.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("text/plain", response.Content.Headers.ContentType.ToString());
            Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
        }
    }
}
