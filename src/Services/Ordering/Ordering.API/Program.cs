using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ordering.Application;
using Ordering.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var orderingEndpointGroup = app.MapGroup("/api/v1/Ordering")
    .WithOpenApi();

orderingEndpointGroup.MapGet("/{username}", async (string username, CancellationToken ct) =>
{

})
    .WithSummary("Get orders for a certain username");

app.Run();
