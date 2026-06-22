using CRM.Data;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("CrmDatabase");
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "crm_auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Wymuszamy autoryzacjê globalnie (wszystkie kontrolery wymagaj¹ uwierzytelnienia),
// ale pozwolimy oznaczaæ akcje jako [AllowAnonymous]
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddAuthorization(options =>
{
    // role-based works too, but definujemy policy per-permission
    options.AddPolicy("CanManageInvoices", policy =>
        policy.RequireClaim("CAN_MANAGE_INVOICES", "true"));

    options.AddPolicy("CanManageEmployees", policy =>
        policy.RequireClaim("CAN_MANAGE_EMPLOYEES", "true"));

    options.AddPolicy("CanManageClients", policy =>
        policy.RequireClaim("CAN_MANAGE_CLIENTS", "true"));

    options.AddPolicy("CanManageServices", policy =>
        policy.RequireClaim("CAN_MANAGE_SERVICES", "true"));

    options.AddPolicy("CanManageReservations", policy =>
        policy.RequireClaim("CAN_MAKE_RESERVATIONS", "true"));

});

var app = builder.Build();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();