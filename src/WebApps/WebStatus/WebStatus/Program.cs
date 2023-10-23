using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddHealthChecksUI(x => x.SetEvaluationTimeInSeconds(10))
    .AddInMemoryStorage();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapHealthChecksUI();

app.Run();
