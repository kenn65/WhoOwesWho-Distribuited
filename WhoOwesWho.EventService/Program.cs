using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using WhoOwesWho.EventService.EfCore.Context;
using WhoOwesWho.EventService.EfCore.Extensions;
using WhoOwesWho.EventService.Middleware;
using WhoOwesWho.EventService.Repositories;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.Gateways;
using WhoOwesWho.EventService.Services.ServiceBus.Publishers;


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


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Authorization:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Authorization:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authorization:JwtSecret"]!)),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddSwaggerGen(options =>
{
    // Bearer token security definition
    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        Description = "Enter your bearer token"
    });

    // API Key security definition
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "Enter your API key",
    });

    // Security requirements for both schemes
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                In = ParameterLocation.Header,
                Name = "X-API-Key"
            },
            new string[] { }
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" },
                In = ParameterLocation.Header,
                Name = "Authorization"
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ConfigureDatabaseAsync();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.EventService API"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();
app.Run();

