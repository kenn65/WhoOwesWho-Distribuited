using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WhoOwesWho.EventService.Middleware;
using WhoOwesWho.EventService.Services;
using WhoOwesWho.EventService.Services.ServiceBus.Handlers;
using WhoOwesWho.EventService.Services.ServiceBus.Handling;
using WhoOwesWho.EventService.Services.ServiceBus.Receivers;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.Currency;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.User;
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

builder.Services.AddSingleton<IEventCurrencyMessageSender, EventCurrencyMessageSender>();
builder.Services.AddSingleton<IProtectValueMessageSender, ProtectValueMessageSender>();
builder.Services.AddSingleton<IUnprotectValueMessageSender, UnprotectValueMessageSender>();
builder.Services.AddSingleton<IEventUserMessageSender, EventUserMessageSender>();

builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();

builder.Services.AddScoped<PaymentEventHandler>();
builder.Services.AddScoped<PaymentEventUsersHandler>();
builder.Services.AddScoped<PaymentUserEventHandler>();
builder.Services.AddScoped<UserEventHandler>();
builder.Services.AddScoped<UserEventUsersHandler>();

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentEventHandler>(EventQueues.PaymentEventRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentEventUsersHandler>(EventQueues.PaymentEventUsersRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentUserEventHandler>(EventQueues.PaymentUserEventRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<UserEventHandler>(EventQueues.UserEventRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<UserEventUsersHandler>(EventQueues.UserEventUsersRequest));

builder.Services.AddHostedService<EventReceiver>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IDataMutationService, DataMutationService>();
builder.Services.AddScoped<IDataQueryService, DataQueryService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();


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
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.EventService API"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();
app.Run();

