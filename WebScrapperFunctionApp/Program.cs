using Microsoft.Extensions.Hosting;
using System.Net;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
    })
    .Build();
SentrySdk.Init(options =>
{
    options.Dsn = "https://e0b23e627e556486307de01a074b16a8@sentry.houselabs.co.za/15";
    options.AutoSessionTracking = true;
    options.IsGlobalModeEnabled = true;
    options.EnableTracing = true;
    //options.Debug = true;
});
host.Run();
