using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WhoOwesWho.CurrencyService.Services;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Handlers;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Receivers;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});
builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();

builder.Services.AddScoped<CurrenciesMessageHandler>();
builder.Services.AddScoped<CurrencyMessageHandler>();
builder.Services.AddScoped<ExchangeRateMessageHandler>();

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<CurrenciesMessageHandler>(CurrencyQueues.CurrenciesRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<CurrencyMessageHandler>(CurrencyQueues.CurrencyRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<ExchangeRateMessageHandler>(CurrencyQueues.ExchangeRateRequest));

builder.Services.AddHostedService<CurrencyReceiver>();

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();


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
            []
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" },
                In = ParameterLocation.Header,
                Name = "Authorization"
            },
            []
        }
    });
});




var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.CurrencyService API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
