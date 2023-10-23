using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Shared.Utilites.HealthChecks;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddHealthChecks();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .AddTransforms(transforms =>
        {
            transforms.AddResponseTransform(transform =>
            {
                var testvalue = "Test-Value";
                transform.ProxyResponse?.Headers.Add("X-YARP-Response-Id", testvalue);
                return ValueTask.CompletedTask;
            });

            transforms.AddRequestTransform(transform =>
            {
                var testvalue = "Test-Value";
                transform.ProxyRequest.Headers.Add("X-YARP-Request-Id", testvalue);
                return ValueTask.CompletedTask;
            });
        });

var app = builder.Build();

app.MapGet("/", () => "Hello YARP!");
app.MapCustomHealthChecks();
app.MapReverseProxy();

app.Run();
