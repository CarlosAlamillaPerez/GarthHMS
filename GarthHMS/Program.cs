using GarthHMS.Core.Interfaces.Repositories;
using GarthHMS.Core.Interfaces.Services;
using GarthHMS.Infrastructure.Data;
using GarthHMS.Infrastructure.Repositories;
using GarthHMS.Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURACIÓN DE SERVICIOS
// ========================================

// Add services to the container.
builder.Services.AddControllersWithViews();

// HttpContextAccessor para MultiTenancyHelper
builder.Services.AddHttpContextAccessor();

// ========================================
// CONFIGURACIÓN DE AUTENTICACIÓN
// ========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "GarthHMS.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ========================================
// CONFIGURACIÓN DE SESIÓN
// ========================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "GarthHMS.Session";
});

// ========================================
// DEPENDENCY INJECTION - INFRASTRUCTURE
// ========================================

// Acceso a datos
builder.Services.AddScoped<BaseDeDatos>();
builder.Services.AddScoped<Procedimientos>();
builder.Services.AddScoped<MultiTenancyHelper>();

// Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

// ========================================
// DEPENDENCY INJECTION - APPLICATION
// ========================================

// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IRoomService, RoomService>();


// ============================================
// CONFIGURACIÓN DE DAPPER
// ============================================
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var assembly = typeof(GarthHMS.Core.Entities.User).Assembly;
var entityTypes = assembly.GetTypes()
    .Where(t => t.Namespace == "GarthHMS.Core.Entities");

foreach (var type in entityTypes)
{
    var map = new Dapper.CustomPropertyTypeMap(
        type,
        (t, columnName) =>
        {
            var prop = t.GetProperties().FirstOrDefault(p =>
            {
                var attr = p.GetCustomAttribute<ColumnAttribute>();
                return attr != null
                    ? attr.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    : p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase);
            });
            return prop;
        }
    );
    Dapper.SqlMapper.SetTypeMap(type, map);
}

// ========================================
// BUILD APP
// ========================================
var app = builder.Build();

// ========================================
// CONFIGURACIÓN DE MIDDLEWARE
// ========================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ========================================
// RUTAS
// ========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========================================
// RUN APP
// ========================================
app.Run();