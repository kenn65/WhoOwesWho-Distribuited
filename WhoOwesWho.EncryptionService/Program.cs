using FluentValidation;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using WhoOwesWho.EncryptionService.Middleware;
using WhoOwesWho.EncryptionService.Services;
using WhoOwesWho.EncryptionService.Validators;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEncryptionSecurityService, EncryptionSecurityService>();
builder.Services.AddValidatorsFromAssemblyContaining<ProtectCookiesRequestValidator>();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Description = "API Key"
        };

        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations!.Values)
            {
                operation.Security ??= new List<OpenApiSecurityRequirement>();
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("ApiKey"),
                        new List<string>()
                    }
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
        options.Title = "WhoOwesWho.EncryptionService API";
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["ApiKey"]
        };
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ApiKeySecurity>();
app.Run();