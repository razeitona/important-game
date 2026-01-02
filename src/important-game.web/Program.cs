using Dapper;
using important_game.infrastructure;
using important_game.web.Handlers;
using important_game.web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for proxy support (HTTPS termination)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                               Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
builder.Services.AddRazorPages();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

#if DEBUG
builder.Services.AddSassCompiler();
#endif

builder.Services.MatchImportanceInfrastructure(builder.Configuration);

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SaveTokens = true;

    // Request user profile information
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // Map claims
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("email_verified", "email_verified");
});

builder.Services.AddHttpContextAccessor();

// Register Background Services
builder.Services.AddHostedService<SitemapGeneratorService>();

Batteries.Init();
SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
SqlMapper.AddTypeHandler(new NullableDateTimeOffsetHandler());

var app = builder.Build();

// Use forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
