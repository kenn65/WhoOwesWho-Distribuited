using Microsoft.OpenApi.Models;
using WhoOwesWho.EncryptionService.Middleware;
using WhoOwesWho.EncryptionService.Services;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IEncryptionSecurityService, EncryptionSecurityService>();

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

