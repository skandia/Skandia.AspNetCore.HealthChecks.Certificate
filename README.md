# Skandia.AspNetCore.HealthChecks.Certificate
This is a module for Microsoft.Extensions.Diagnostics.HealthChecks that allows you to do a check regarding your client certificates via thumbprint.

## Getting Started
Install the nuget package Skandia.AspNetCore.HealthChecks.Certificate
```
Install-Package Skandia.AspNetCore.HealthChecks.Certificate
```
Then in your Startup.cs, simply add your certificate check as well.
``` csharp
public void ConfigureServices(IServiceCollection services)
    {
        ...
        services
            .AddHealthChecks() // using Microsoft.AspNetCore.Diagnostics.HealthChecks
            .AddClientCertificateCheck("MyFriendlyCertCheckName", "MyCertThumbprint"); // using Skandia.Extensions.Diagnostics.HealthChecks.Certificate
        ...
    }
```


## Configuration
This is all variables you can change with its default value:
``` csharp
services.AddHealthChecks()
.AdClientCertificateCheck("MyCertCheckName", "MyCertThumbprint",
    mustHavePrivateKey: false,
    storeName: StoreName.My,
    storeLocation: StoreLocation.LocalMachine,
    validOnly: true,
    failureStatus: HealthStatus.Unhealthy,
    tags: null
);
```

