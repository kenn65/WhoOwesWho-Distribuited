using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
using WhoOwesWho.PaymentService.EfCore.Context;
using WhoOwesWho.PaymentService.EfCore.Extensions;
using WhoOwesWho.PaymentService.Middleware;
using WhoOwesWho.PaymentService.Repositories;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.Gateways;
using WhoOwesWho.PaymentService.Services.ServiceBus.Receivers;
using WhoOwesWho.PaymentService.Services.ServiceBus.Resolvers;
using static WhoOwesWho.PaymentService.Services.IPaymentCalculationService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("whooweswho-payments")));

builder.AddServiceDefaults();

builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetConnectionString("redis-cache");
    return ConnectionMultiplexer.Connect(configuration!);
});

builder.Services.AddSingleton(sp =>
{
    var mux = sp.GetRequiredService<IConnectionMultiplexer>();
    return mux.GetDatabase();
});

// Add services to the container.
builder.Services.AddSingleton<EventReceiver>();
builder.Services.AddHostedService<EventReceiverStartupService>();
builder.Services.AddScoped<IEventResolverService, EventResolverService>();

builder.Services.AddScoped<IUserBalanceService, UserBalanceService>();
builder.Services.AddScoped<IPaymentLookupService, PaymentLookupService>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IPaymentSecurityService, PaymentSecurityService>();
builder.Services.AddScoped<IPaymentCalculationService, PaymentCalculationService>();
builder.Services.AddScoped<IPaymentQueryRepository, PaymentQueryRepository>();
builder.Services.AddScoped<IPaymentMutationRepository , PaymentMutationRepository>();
builder.Services.AddScoped<IPaymentCacheRepository, PaymentCacheRepository>();
builder.Services.AddScoped<ICurrencyGatewayService, CurrencyGatewayService>();
builder.Services.AddScoped<IEncryptionGatewayService, EncryptionGatewayService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 🔐 Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authorization:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authorization:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Authorization:JwtSecret"]!)
            )
        };
    });


// 📄 OpenAPI (Scalar)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        // 🔑 API Key
        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Description = "API Key"
        };

        // 🔐 Bearer
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Description = "Bearer token"
        };

        // 👉 OR logic (ApiKey OR Bearer)
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations!.Values)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("ApiKey"), new List<string>() }
                });

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
                });
            }
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "WhoOwesWho.PaymentService API";

        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["ApiKey", "Bearer"]
        };
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeySecurity>();
app.Run();