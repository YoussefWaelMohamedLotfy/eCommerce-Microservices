using Catalog.API.Data;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Shared.Utilites.HealthChecks;
using Shared.Utilites.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Serilogger.Configure);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(o => o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3);
    options.ConfigureHttpsDefaults(o => o.AllowAnyClientCertificate());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["JWT:ValidIssuer"];

        options.RequireHttpsMetadata = builder.Environment.IsProduction();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

builder.Services.AddScoped<ICatalogContext, CatalogContext>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddOutputCache();

builder.Services.AddHealthChecks()
    .AddMongoDb(builder.Configuration["DatabaseSettings:ConnectionString"]!, "MongoDb Health", HealthStatus.Unhealthy)
    .AddIdentityServer(new Uri(builder.Configuration["JWT:ValidIssuer"]!), name: "Duende IdentityServer Health", failureStatus: HealthStatus.Degraded)
    .AddElasticsearch(builder.Configuration["Serilog:WriteTo:1:Args:nodeUris"]!, "Elasticsearch Health", HealthStatus.Degraded, timeout: TimeSpan.FromSeconds(2));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("DockerDevelopment"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    IdentityModelEventSource.ShowPII = true;
}

app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

var catalogEndpointGroup = app.MapGroup("/api/v1/Catalog")
    .RequireAuthorization("ApiScope")
    .WithOpenApi();

catalogEndpointGroup.MapGet("/", async (IProductRepository repo, CancellationToken ct) =>
{
    var products = await repo.GetProducts(ct).ConfigureAwait(false);
    return Results.Ok(products);
})
    .WithSummary("Get all products");

catalogEndpointGroup.MapGet("/{id:length(24)}", async (string id, IProductRepository repo, CancellationToken ct) =>
{
    var product = await repo.GetProduct(id, ct).ConfigureAwait(false);
    return Results.Ok(product);
})
    .CacheOutput()
    .WithName("GetProduct")
    .WithSummary("Get product with an ID");

catalogEndpointGroup.MapGet("/Category/{category}", async (string category, IProductRepository repo,
    CancellationToken ct) =>
{
    var products = await repo.GetProductByCategory(category, ct).ConfigureAwait(false);
    return Results.Ok(products);
})
    .WithSummary("Get products for a category");

catalogEndpointGroup.MapPost("/", async (Product newProduct, IProductRepository repo, CancellationToken ct) =>
{
    await repo.CreateProduct(newProduct, ct).ConfigureAwait(false);
    return Results.CreatedAtRoute("GetProduct", new { id = newProduct.Id }, newProduct);
})
    .WithSummary("Creates a new product in catalog");

catalogEndpointGroup.MapPut("/", async (Product updatedProduct, IProductRepository repo, CancellationToken ct)
    => Results.Ok(await repo.UpdateProduct(updatedProduct, ct).ConfigureAwait(false)))
    .WithSummary("Updates existing product in catalog");

catalogEndpointGroup.MapDelete("/{id:length(24)}", async (string id, IProductRepository repo, CancellationToken ct) =>
{
    var isDeleted = await repo.DeleteProduct(id, ct).ConfigureAwait(false);
    return isDeleted ? Results.NoContent() : Results.NotFound();
})
    .WithSummary("Deletes a product from catalog");

app.MapCustomHealthChecks();

app.Run();
