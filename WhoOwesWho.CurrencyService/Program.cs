using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;
using System.Text;
using WhoOwesWho.CurrencyService.Middleware;
using WhoOwesWho.CurrencyService.Repositories;
using WhoOwesWho.CurrencyService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

builder.Services.AddScoped<ICurrencyService, CurrencyService>();
//builder.Services.AddScoped<ICurrencySecurityService, CurrencySecurityService>();
builder.Services.AddScoped<ICurrencyCacheRepository, CurrencyCacheRepository>();

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
        options.Title = "WhoOwesWho.CurrrencyService API";

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