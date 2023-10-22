using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddHealthChecksUI(x => x.AddHealthCheckEndpoint("IS", "https://localhost:5001/health"))
    .AddInMemoryStorage();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecksUI();

app.Run();
