using Azure.Messaging.ServiceBus;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
using WhoOwesWho.EventService.EfCore.Context;
using WhoOwesWho.EventService.EfCore.Extensions;
using WhoOwesWho.EventService.Middleware;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.EventService.Services.ServiceBus.Publishers;
using WhoOwesWho.EventService.Validators;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EventDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("whooweswho-events")));

builder.AddServiceDefaults();

// Add services to the container.
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

builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<IEventLookupService, EventLookupService>();
builder.Services.AddScoped<IEventCommandService, EventCommandService>();
builder.Services.AddScoped<IEventSecurityService, EventSecurityService>();
builder.Services.AddScoped<IEventPublishingService, EventPublishingService>();
builder.Services.AddScoped<IUserCacheService, UserCacheService>();
builder.Services.AddScoped<IEventMutationRepository, EventMutationRepository>();
builder.Services.AddScoped<IEventQueryRepository, EventQueryRepository>();
builder.Services.AddScoped<IEventCacheRepository, EventCacheRepository>();
builder.Services.AddScoped<ICurrencyGatewayService, CurrencyGatewayService>();
builder.Services.AddScoped<IEncryptionGatewayService, EncryptionGatewayService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EventAssignmentRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EventUnassignmentRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateEventRequestValidator>();

builder.Services.AddControllers();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "WhoOwesWho.EventService API";

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
app.UseMiddleware<ApiKeySecirity>();
app.Run();
