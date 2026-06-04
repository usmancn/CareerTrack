using CareerTrack.Data;
using CareerTrack.Models.Constants;
using CareerTrack.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// SQLite veritabanı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();

app.Use(async (context, next) =>
{
    var requiresPasswordChange = context.User.Identity?.IsAuthenticated == true &&
        context.User.HasClaim(AppClaims.RequiresPasswordChange, "true");
    var employerPendingApproval = context.User.Identity?.IsAuthenticated == true &&
        context.User.HasClaim(AppClaims.EmployerPendingApproval, "true");
    var isPasswordAllowedPath = context.Request.Path.StartsWithSegments("/Profile/Settings") ||
        context.Request.Path.StartsWithSegments("/Account/Logout") ||
        context.Request.Path.StartsWithSegments("/Account/AccessDenied");
    var isEmployerPendingAllowedPath = context.Request.Path.StartsWithSegments("/Account/PendingApproval") ||
        context.Request.Path.StartsWithSegments("/Account/Logout") ||
        context.Request.Path.StartsWithSegments("/Account/AccessDenied");

    if (requiresPasswordChange && !isPasswordAllowedPath)
    {
        context.Response.Redirect("/Profile/Settings");
        return;
    }

    if (employerPendingApproval && !isEmployerPendingAllowedPath)
    {
        context.Response.Redirect("/Account/PendingApproval");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// ── Rol ve Admin Seed ─────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedData.Initialize(roleManager, userManager, db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migration/seed hatası: {Message}", ex.Message);
    }
}

app.Run();
