using Azure.Messaging.ServiceBus;
using Microsoft.OpenApi.Models;
using WhoOwesWho.AuthorizationService.Middleware;
using WhoOwesWho.AuthorizationService.Services;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Encryption;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.Messaging;
using WhoOwesWho.AuthorizationService.Services.ServiveBus.Senders.User;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IProtectCookiesMessageSender, ProtectCookiesMessageSender>();
builder.Services.AddSingleton<IUnprotectValueMessageSender, UnprotectValueMessageSender>();
builder.Services.AddSingleton<IAuthenticationMessageSender, AuthenticationMessageSender>();
builder.Services.AddSingleton<IUserMessageSender, UserMessageSender>();

builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IValidationService, ValidationService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.AuthorizationService API"));
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();

app.Run();
    
