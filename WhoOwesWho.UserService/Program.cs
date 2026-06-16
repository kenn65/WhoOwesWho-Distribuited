using Azure.Messaging.ServiceBus;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
using WhoOwesWho.UserService.EfCore.Context;
using WhoOwesWho.UserService.EfCore.Extensions;
using WhoOwesWho.UserService.Middleware;
using WhoOwesWho.UserService.Repositories;
using WhoOwesWho.UserService.Services;
using WhoOwesWho.UserService.Services.Gateways;
using WhoOwesWho.UserService.Services.ServiceBus.Publishers;
using WhoOwesWho.UserService.Validators;
using static WhoOwesWho.UserService.Repositories.IUserCacheRepository;

var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("whooweswho-users")));

builder.AddServiceDefaults();

// Service Bus
builder.Services.AddSingleton(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("servicebus");
    return new ServiceBusClient(connectionString);
});

// Redis
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

// Messaging
builder.Services.AddSingleton<IMessagingPublisher, MessagingPublisher>();
builder.Services.AddSingleton<IUserPublisher, UserPublisher>();

// Services
builder.Services.AddScoped<IUserPublishingService, UserPublishingService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();
builder.Services.AddScoped<IUserQueryRepository, UserQueryRepository>();
builder.Services.AddScoped<IUserMutationRepository, UserMutationRepository>();
builder.Services.AddScoped<IUserCacheRepository, UserCacheRepository>();
builder.Services.AddScoped<IUserCreationService, UserCreationService>();
builder.Services.AddScoped<IUserValidationService, UserValidationService>();
builder.Services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
builder.Services.AddScoped<IResetPasswordService, ResetPasswordService>();
builder.Services.AddScoped<IChangePasswordService, ChangePasswordService>();
builder.Services.AddScoped<IUserNotificationService, UserNotificationService>();
builder.Services.AddScoped<IUserSecurityService, UserSecurityService>();
builder.Services.AddScoped<IEncryptionGatewayService, EncryptionGatewayService>();
builder.Services.AddScoped<IEventGatewayService, EventGatewayService>();
builder.Services.AddScoped<IEmailValidationService, EmailValidationService>();
builder.Services.AddScoped<IUserUpdateValidationService, UserUpdateValidationService>();
builder.Services.AddValidatorsFromAssemblyContaining<SignUpRequestValidatior>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ForgotPasswordRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ResetPasswordRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SignUpRequestValidatior>();
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
    await app.ConfigureDatabaseAsync();

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "WhoOwesWho.UserService API";

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