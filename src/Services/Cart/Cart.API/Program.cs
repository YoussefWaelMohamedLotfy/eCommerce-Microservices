using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.Services.AddStackExchangeRedisCache(x =>
{
    x.InstanceName = "Cart.API";
    x.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

var carEndpointGroup = app.MapGroup("/api/v1/Cart")
    .WithOpenApi();

carEndpointGroup.MapGet("/{username}", async () =>
{

});

app.Run();
