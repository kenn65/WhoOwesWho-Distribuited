using Azure.Messaging.ServiceBus;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using WhoOwesWho.MessagingService.Middleware;
using WhoOwesWho.MessagingService.Services;
using WhoOwesWho.MessagingService.Services.Gateways;
using WhoOwesWho.MessagingService.Services.ServiceBus.Handling;
using WhoOwesWho.MessagingService.Services.ServiceBus.Receivers;
using WhoOwesWho.MessagingService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("servicebus");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<MessagingReceiver>();
builder.Services.AddHostedService<MessagingReceiverStartupService>();
builder.Services.AddScoped<IEmailMessagingService, EmailMessagingService>();
builder.Services.AddScoped<IMessagingSecurityService, MessagingSecurityService>();
builder.Services.AddScoped<IEncryptionGatewayService, EncryptionGatewayService>();
builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();
builder.Services.AddValidatorsFromAssemblyContaining<MessagingRequestValidator>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "WhoOwesWho.MessagingService API";

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