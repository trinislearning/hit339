using EasyGames.Data;
using EasyGames.Models;
using EasyGames.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use SQLite for Development, SQL Server for Production
if (builder.Environment.IsDevelopment())
{
    var cs = builder.Configuration.GetConnectionString("DevSqlite") ?? "Data Source=EasyGames.db";
    builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(cs));
}
else
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(cs));
}

// ---- Identity ----
builder.Services.AddDefaultIdentity<ApplicationUser>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ---- App Services ----
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(4);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();

// (Add these if you’re using the new modules)
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IPromoService, PromoService>();
builder.Services.AddScoped<EasyGames.Services.EmailService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Shop}/{action=Index}/{id?}");
app.MapRazorPages();

// ---- Auto-migrate + seed ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();                    // apply migrations
    await IdentitySeed.SeedAsync(scope.ServiceProvider); // roles, owner, products
}

app.Run();
