
using WhoOwesWho.WebApp.Components;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Cookies;
using WhoOwesWho.WebApp.Infrastructure.Account;
using WhoOwesWho.WebApp.Infrastructure.Protection;
using WhoOwesWho.WebApp.Services;
using WhoOwesWho.WebApp.UseCases.Account;
using WhoOwesWho.WebApp.UseCases.Account.PluginInterfaces;
using WhoOwesWho.WebApp.UseCases.Protection;
using WhoOwesWho.WebApp.UseCases.Protection.PluginInterfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<IAuthorizationUseCase, AuthorizationUseCase>();
builder.Services.AddTransient<IAuthorize, AuthorizationPlugin>();
builder.Services.AddTransient<IProtectionUseCase, ProtectionUseCase>();
builder.Services.AddTransient<IProtection, ProtectionPlugin>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<ICookieClientService, CookieClientService>();
builder.Services.AddTransient<IUserUseCase, UserUseCase>();
builder.Services.AddTransient<IUser, UserPlugin>();
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


// GET COOKIES
app.MapGet("/api/auth/get-cookies", (HttpContext ctx) =>
{
    return Results.Ok(new CookiesResponseModel
    {
        TokenValue = ctx.Request.Cookies[".WhoOwesWho.Token"] ?? "",
        UserIdValue = ctx.Request.Cookies[".WhoOwesWho.UserId"] ?? "",
        UserEmailAddressValue = ctx.Request.Cookies[".WhoOwesWho.Email"] ?? "",
        AdminValue = ctx.Request.Cookies[".WhoOwesWho.UserAdmin"] ?? "",
    });
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
