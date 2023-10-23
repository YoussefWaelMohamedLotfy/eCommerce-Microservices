using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddHealthChecksUI(x => x.SetEvaluationTimeInSeconds(10))
    .AddInMemoryStorage();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecksUI();

app.Run();
