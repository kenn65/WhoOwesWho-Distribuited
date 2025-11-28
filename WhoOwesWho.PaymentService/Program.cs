using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WhoOwesWho.EventService.Services.ServiceBus.Senders.User;
using WhoOwesWho.PaymentService.Middleware;
using WhoOwesWho.PaymentService.Services;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Currency;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Event;
using WhoOwesWho.UserService.Services.ServiceBus.Senders.Event;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("sbemulatorns");
    return new ServiceBusClient(connectionString);
});

builder.Services.AddSingleton<IPaymentCurrenciesMessageSender, PaymentCurrenciesMessageSender>();
builder.Services.AddSingleton<IPaymentExchangeRateMessageSender, PaymentExchangeRateMessageSender> ();
builder.Services.AddSingleton<IProtectValueMessageSender, ProtectValueMessageSender>();
builder.Services.AddSingleton<IUnprotectValueMessageSender, UnprotectValueMessageSender>();
builder.Services.AddSingleton<IPaymenEventUsersMessageSender, PaymenEventUsersMessageSender>();
builder.Services.AddSingleton<IPaymentEventMessageSender, PaymentEventMessageSender>();
builder.Services.AddSingleton<IPaymentUserEventMessageSender, PaymentUserEventMessageSender>();
builder.Services.AddSingleton<IPaymentUserMessageSender, PaymentUserMessageSender>();

builder.Services.AddScoped<IUserBalanceService, UserBalanceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentDetailsService, PaymentDetailsService>();
builder.Services.AddScoped<IDataQueryService, DataQueryService>();
builder.Services.AddScoped<IDataMutationService, DataMutationService>();
builder.Services.AddControllers();
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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.PaymentService API"));
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();
app.Run();