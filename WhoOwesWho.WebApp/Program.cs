using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WhoOwesWho.WebApp.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.CoreBusiness.Interfaces;
using WhoOwesWho.WebApp.Infrastructure.Account;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Infrastructure.Events;
using WhoOwesWho.WebApp.Infrastructure.Payments;
using WhoOwesWho.WebApp.Infrastructure.Protection;
using WhoOwesWho.WebApp.Infrastructure.Services;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.StateHandlers;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Currencies;
using WhoOwesWho.WebApp.UseCases.Currencies.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Events;
using WhoOwesWho.WebApp.UseCases.Events.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Payments;
using WhoOwesWho.WebApp.UseCases.Payments.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;
using WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// =====================================
// COMPONENTS
// =====================================

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// =====================================
// AUTHENTICATION
// =====================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Authorization:Issuer"],

                ValidAudience =
                    builder.Configuration["Authorization:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Authorization:JwtSecret"]!)),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token =
                    context.Request.Cookies[
                        ".WhoOwesWho.Token"];

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(context.Exception);

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// =====================================
// GENERAL SERVICES
// =====================================

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHostNameService, HostNameService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpClient();

// =====================================
// STATE HANDLERS
// =====================================

builder.Services.AddSingleton<IStateHandler<EventModel>, StateHandler<EventModel>>();
builder.Services.AddSingleton<IStateHandler<PaymentStateModel>, StateHandler<PaymentStateModel>>();

// =====================================
// USE CASES + PLUGINS
// =====================================

builder.Services.AddTransient<IAuthorizationUseCase, AuthorizationUseCase>();
builder.Services.AddTransient<IAuthorizationPlugin, AuthorizationPlugin>();
builder.Services.AddTransient<IProtectionUseCase, ProtectionUseCase>();
builder.Services.AddTransient<IProtectionPlugin, ProtectionPlugin>();
builder.Services.AddTransient<IUserUseCase, UserUseCase>();
builder.Services.AddTransient<IUserPlugin, UserPlugin>();
builder.Services.AddTransient<IEventsUseCase, EventsUseCase>();
builder.Services.AddTransient<IEventsPlugin, EventsPlugin>();
builder.Services.AddTransient<ICurrenciesUseCase, CurrenciesUseCase>();
builder.Services.AddTransient<ICurrencyPlugin, CurrencyPlugin>();
builder.Services.AddTransient<IPaymentsUseCase, PaymentsUseCase>();
builder.Services.AddTransient<IPaymentsPlugin, PaymentsPlugin>();

var app = builder.Build();

app.MapDefaultEndpoints();

// =====================================
// COOKIE ENDPOINTS
// =====================================

app.MapPost(
    "/api/auth/set-cookies",
    (CookiesResponseModel data,
     HttpContext ctx) =>
    {
        // =====================================
        // JWT ACCESS TOKEN COOKIE
        // =====================================

        var tokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",

            // SHORT LIFETIME
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        // =====================================
        // REFRESH TOKEN COOKIE
        // =====================================

        var refreshOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",

            // LONG LIFETIME
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };

        void Set(
            string name,
            string value,
            CookieOptions options)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                ctx.Response.Cookies.Append(
                    name,
                    value,
                    options);
            }
        }

        //=====================================
        //ACCESS TOKEN
        //=====================================

        Set(
            data.TokenName,
            data.TokenValue,
            tokenOptions);

        //=====================================
        //REFRESH TOKEN
        //=====================================

        Set(
            data.RefreshName,
            data.RefreshValue,
            refreshOptions);

        return Results.Ok();
    });

app.MapPost("/api/auth/delete-cookies",
    (HttpContext ctx) =>
    {
        var names = new[]
        {
            ".WhoOwesWho.Token",
            ".WhoOwesWho.Refresh"
        };

        foreach (var name in names)
        {
            ctx.Response.Cookies.Delete(name);
        }

        return Results.Ok();
    });

// =====================================
// PIPELINE
// =====================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();