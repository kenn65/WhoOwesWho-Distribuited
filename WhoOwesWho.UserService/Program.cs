using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WhoOwesWho.Models.Models.Base.ServiceBus;
using WhoOwesWho.PaymentService.Services.ServiceBus.Senders.Encryption;
using WhoOwesWho.UserService.Middleware;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Services.ServiceBus.Handlers;
using WhoOwesWho.UserService.Services.ServiceBus.Receivers;
using WhoOwesWho.UserService.Services.ServiceBus.Resolvers;
using WhoOwesWho.UserService.Services.ServiceBus.Senders.Event;
using WhoOwesWho.UserService.Services.ServiceBus.Senders.Messaging;
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
builder.Services.AddSingleton<IUnprotectValueMessageSender, UnprotectValueMessageSender>();
builder.Services.AddSingleton<IUserEventUsersMessageSender, UserEventUsersMessageSender>();
builder.Services.AddSingleton<IUserEventMessageSender, UserEventMessageSender>();
builder.Services.AddSingleton<IForgotPasswordMessageSender, ForgotPasswordMessageSender>();
builder.Services.AddSingleton<ISignUpMessageSender, SignUpMessageSender>();

builder.Services.AddScoped<IMessageResolverService, MessageResolverService>();

builder.Services.AddScoped<AuthorizationUserHandler>();
builder.Services.AddScoped<EventUserHandler>();
builder.Services.AddScoped<PaymentUserHandler>();

builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<AuthorizationUserHandler>(UserQueues.AuthorizationUserRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<EventUserHandler>(UserQueues.EventUserRequest));
builder.Services.AddSingleton<IQueueHandlerRegistration>(new QueueHandlerRegistration<PaymentUserHandler>(UserQueues.PaymentUserRequest));

builder.Services.AddHostedService<UserReceiver>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDataMutationService, DataMutationService>();
builder.Services.AddScoped<IDataQueryService, DataQueryService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
builder.Services.AddScoped<IChangePasswordService, ChangePasswordService>();


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
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/Swagger/v1/swagger.json", "WhoOwesWho.UserService API"));
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeyMiddleware>();
app.Run();
