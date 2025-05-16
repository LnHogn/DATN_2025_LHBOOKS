using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LHBooksWeb.Data;
using LHBooksWeb.Models;
using LHBooksWeb.Services;
using LHBooksWeb.Services.Email;
using LHBooksWeb.Services.ProductHome;
using LHBooksWeb.Services.Vnpay;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;

//using LHBooksWeb.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    options.Events.OnRedirectToLogin = context =>
    {
        var request = context.Request;
        var path = request.Path;

        // Nếu là request Ajax, trả về 401 thay vì redirect
        if (request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        // Nếu là khu vực Admin
        if (path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Admin/Account/LoginAdmin?returnUrl=" + Uri.EscapeDataString(request.Path));
        }
        else
        {
            context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(request.Path));
        }

        return Task.CompletedTask;
    };


    options.Events.OnRedirectToAccessDenied = context =>
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Redirect("/Admin/Account/AccessDenied");
        }
        else
        {
            context.Response.Redirect("/Account/AccessDenied");
        }

        return Task.CompletedTask;
    };
});




builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // Thời gian hết hạn của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
//builder.Services.AddSession(options =>
//{
//    options.IdleTimeout = TimeSpan.FromMinutes(30);
//    options.Cookie.IsEssential = true;
//});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddSingleton<IProvinceService, ProvinceService>();
builder.Services.AddTransient<LHBooksWeb.Services.Email.IEmailSender, EmailSender>();


//vnpay
builder.Services.AddSingleton<IVnPayService, VnPayService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

 // Cho phép truy cập file tĩnh từ wwwroot

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads")),
    RequestPath = "/Uploads"
});



app.MapControllerRoute(
    name: "CheckOut",
    pattern: "thanh-toan",
    defaults: new { controller = "ShoppingCart", action = "CheckOut" });

app.MapControllerRoute(
    name: "vnpay_return",
    pattern: "vnpay_return",
    defaults: new { controller = "ShoppingCart", action = "VnpayReturn" });

app.MapControllerRoute(
    name: "ShoppingCart",
    pattern: "gio-hang",
    defaults: new { controller = "ShoppingCart", action = "Index" });

app.MapControllerRoute(
    name: "category",
    pattern: "ProductCategory/Index/{alias}/{id:int}",
    defaults: new { controller = "ProductCategory", action = "Index" });


app.MapControllerRoute(
    name: "subcategory",
    pattern: "ProductSubCategory/Index/{alias}/{id:int}",
    defaults: new { controller = "ProductSubCategory", action = "Index" });

app.MapControllerRoute(
    name: "productDetail",
    pattern: "Products/Detail/{alias}/{id}",
    defaults: new { controller = "Products", action = "Detail" }
);


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
