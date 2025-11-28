using Azure.Messaging.ServiceBus;
using Microsoft.OpenApi.Models;
using WhoOwesWho.MessagingService.Middleware;
using WhoOwesWho.MessagingService.Services;
using WhoOwesWho.MessagingService.Services.ServiceBus.Handlers;
using WhoOwesWho.MessagingService.Services.ServiceBus.Handling;
using WhoOwesWho.MessagingService.Services.ServiceBus.Receivers;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using WhoOwesWho.UserService.Services.ServiceBus.Senders.Encryption;
using static WhoOwesWho.Models.Models.Base.Queues;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IProtectValueMessageSender, ProtectValueMessageSender>();

builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();

builder.Services.AddScoped<AuthenticationValidationHandler>();
builder.Services.AddScoped<ForgotPasswordHandler>();
builder.Services.AddScoped<SignUpHandler>();

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<AuthenticationValidationHandler>(MessagingQueues.AuthenticationValidateRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<ForgotPasswordHandler>(MessagingQueues.ForgotPasswordRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<SignUpHandler>(MessagingQueues.SignUpRequest));

builder.Services.AddHostedService<MessagingReceiver>();

builder.Services.AddScoped<IEmailMessagingService, EmailMessagingService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();


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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.MessagingService API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();
app.Run();

