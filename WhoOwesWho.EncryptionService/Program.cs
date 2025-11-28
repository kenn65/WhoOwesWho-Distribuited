using Azure.Messaging.ServiceBus;
using Microsoft.OpenApi.Models;
using WhoOwesWho.CurrencyService.Services.ServiceBus.Receivers;
using WhoOwesWho.EncryptionService.Middleware;
using WhoOwesWho.EncryptionService.Services;
using WhoOwesWho.EncryptionService.Services.ServiceBus.Handlers;
using WhoOwesWho.EncryptionService.Services.ServiceBus.Resolvers;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using static WhoOwesWho.Models.Models.Base.Queues;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});


builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();

builder.Services.AddScoped<AuthorizationProtectCookiesMessageHandler>();
builder.Services.AddScoped<AuthorizationUnprotectValueMessageHandler>();
builder.Services.AddScoped<EventProtectValueMessageHandler>();
builder.Services.AddScoped<EventUnprotectVaueMessageHandler>();
builder.Services.AddScoped<PaymentProtectValueMessageHandler>();
builder.Services.AddScoped<PaymentUnprotectValueMessageHandler>();
builder.Services.AddScoped<UserProtectValueMessageHandler>();
builder.Services.AddScoped<UserUnprotectValueMessageHandler>();

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<AuthorizationProtectCookiesMessageHandler>(EncryptionQueues.AuthorizationProtectCookiesRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<AuthorizationUnprotectValueMessageHandler>(EncryptionQueues.AuthorizationUnprotectRequest));

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<EventProtectValueMessageHandler>(EncryptionQueues.EventProtectRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<EventUnprotectVaueMessageHandler>(EncryptionQueues.EventUnprotectRequest));

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentProtectValueMessageHandler>(EncryptionQueues.PaymentProtectReuest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentUnprotectValueMessageHandler>(EncryptionQueues.PaymentUnprotectRequest));

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<UserProtectValueMessageHandler>(EncryptionQueues.UserProtectRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<UserUnprotectValueMessageHandler>(EncryptionQueues.UserUnprotectRequest));

builder.Services.AddHostedService<EncryptionReceiver>();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.EncryptionService API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();

app.Run();

