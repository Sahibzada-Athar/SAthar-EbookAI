﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();  // ✅ add session support

// ✅ Configure MySQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34)) // adjust version to your MySQL
    ));

// ✅ Enable Authentication with Cookie Scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        // where to go if not logged in
        options.LogoutPath = "/Account/Logout";      // logout URL
        options.AccessDeniedPath = "/Account/AccessDenied"; // unauthorized
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // session timeout
        options.SlidingExpiration = true;            // extend session if active
    });

// ✅ Authorization middleware (roles, policies etc.)
builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserService, UserService>();
// Add EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
// Add BookService
builder.Services.AddScoped<IBookService, BookService>();
// Add PlanService
builder.Services.AddScoped<IPlansService, PlansService>();
// Add FeatureCartService
builder.Services.AddScoped<IFeatureCartService, FeatureCartService>();
// Add CheckoutService
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
// Register Plan Features Service
builder.Services.AddScoped<IPlanFeaturesService, PlanFeaturesService>();
//
builder.Services.AddScoped<IAPIRawResponseService, APIRawResponseService>();
// Add this to your services
builder.Services.AddScoped<BookProcessingService>();
builder.Services.AddScoped<IAuthorPlansService, AuthorPlansService>();
builder.Services.AddScoped<IAuthorBillsService, AuthorBillsService>();
builder.Services.AddScoped<IAuthorPlansService, AuthorPlansService>();

// Add session services
builder.Services.AddDistributedMemoryCache();
//Register HttpClient factory
builder.Services.AddHttpClient();
//Register Sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // ✅ must be after UseRouting and before UseEndpoints


// ✅ Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();
//app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Role-based dashboard routes
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Dashboard}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "author",
    pattern: "Author/{action=Dashboard}/{id?}",
    defaults: new { controller = "Author" });

app.MapControllerRoute(
    name: "reader",
    pattern: "Reader/{action=Dashboard}/{id?}",
    defaults: new { controller = "Reader" });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapControllerRoute(
    name: "features",
    pattern: "Features/{action=Index}/{id?}",
    defaults: new { controller = "Features" });

app.MapControllerRoute(
    name: "checkout",
    pattern: "Checkout/{action}/{id?}",
    defaults: new { controller = "Checkout", action = "GetPublishableKey" });

app.Run();