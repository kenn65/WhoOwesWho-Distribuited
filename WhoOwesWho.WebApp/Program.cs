
using WhoOwesWho.WebApp.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Events;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;
using WhoOwesWho.WebApp.Infrastructure.Account;
using WhoOwesWho.WebApp.Infrastructure.Currencies;
using WhoOwesWho.WebApp.Infrastructure.Events;
using WhoOwesWho.WebApp.Infrastructure.Payments;
using WhoOwesWho.WebApp.Infrastructure.Protection;
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

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IAuthorizationUseCase, AuthorizationUseCase>();
builder.Services.AddTransient<IAuthorizationPlugin, AuthorizationPlugin>();
builder.Services.AddTransient<IProtectionUseCase, ProtectionUseCase>();
builder.Services.AddTransient<IProtectionPlugin, ProtectionPlugin>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<ICookiesMasterService, CookiesMasterService>();
builder.Services.AddScoped<IHostNameService, HostNameService>();
builder.Services.AddSingleton<IStateHandler<EventModel>, StateHandler<EventModel>>();
builder.Services.AddTransient<IUserUseCase, UserUseCase>();
builder.Services.AddTransient<IUserPlugin, UserPlugin>();
builder.Services.AddTransient<IEventsUseCase, EventsUseCase>();
builder.Services.AddTransient<IEventsPlugin, EventsPlugin>();
builder.Services.AddTransient<ICurrenciesUseCase, CurrenciesUseCase>();
builder.Services.AddTransient<ICurrencyPlugin, CurrencyPlugin>();
builder.Services.AddTransient<IPaymentsUseCase, PaymentsUseCase>();
builder.Services.AddTransient<IPaymentsPlugin, PaymentsPlugin>();
builder.Services.AddSingleton<IStateHandler<PaymentStateModel>, StateHandler<PaymentStateModel>>();

var app = builder.Build();

app.MapDefaultEndpoints();

// SET COOKIES
app.MapPost("/api/auth/set-cookies", (CookiesResponseModel data, HttpContext ctx) =>
{
    var options = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddHours(48)
    };

    void Set(string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ctx.Response.Cookies.Append(name, value, options);
    }

    Set(data.TokenName, data.TokenValue);
    Set(data.UserIdName, data.UserIdValue);
    Set(data.UserEmailAddressName, data.UserEmailAddressValue);
    Set(data.AdminName, data.AdminValue);

    return Results.Ok();
});

// UPDATE ADMIN COOKIE
app.MapPost("/api/auth/update-admin-cookie", (CookiesResponseModel data, HttpContext ctx) =>
{
    var options = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = "/",
        Expires = DateTimeOffset.UtcNow.AddHours(48)
    };

    void Set(string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            ctx.Response.Cookies.Append(name, value, options);
    }

    void Delete(string name)
    {
        ctx.Response.Cookies.Delete(name);
    }

    Delete(data.AdminName);
    Set(data.AdminName, data.AdminValue);

    return Results.Ok();
});

// DELETE COOKIES
app.MapPost("/api/auth/delete-cookies", (HttpContext ctx) =>
{
    var names = new[]
    {
        ".WhoOwesWho.Token",
        ".WhoOwesWho.UserId",
        ".WhoOwesWho.Email",
        ".WhoOwesWho.UserAdmin"
    };

    foreach (var name in names)
        ctx.Response.Cookies.Delete(name);

    return Results.Ok();
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();
app.Run();
