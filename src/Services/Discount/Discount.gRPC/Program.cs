using Discount.gRPC.Data;
using Discount.gRPC.Extensions;
using Discount.gRPC.Repositories;
using Discount.gRPC.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddSingleton<DiscountMapper>();

builder.Services.AddGrpc()
    .AddJsonTranscoding()
    ;
builder.Services.AddGrpcReflection()
    .AddGrpcSwagger();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MigrateDatabase<Program>();
}

// Configure the HTTP request pipeline.
app.MapGrpcService<DiscountService>();
app.MapGrpcReflectionService();
app.MapGet("/", () => "Communication with server is available as gRPC endpoints and REST API.");

app.Run();
