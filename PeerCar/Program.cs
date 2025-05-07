using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Ensure this namespace is included

var builder = WebApplication.CreateBuilder(args);

// إعداد قاعدة البيانات (التأكد من استخدام الـ Dev و Prod Connections حسب البيئة)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString(
        builder.Environment.IsDevelopment() ? "DevConnection" : "ProdConnection")));

// إضافة خدمات المصادقة Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>() // Ensure this method is available
    .AddDefaultTokenProviders(); // لتوليد الرموز (مثل الرموز الخاصة بإعادة تعيين كلمة المرور)

builder.Services.AddControllersWithViews(); // إضافة خدمات الـ MVC

var app = builder.Build();

// تأكد من أن قاعدة البيانات تم إنشاؤها في بداية تشغيل التطبيق
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // تأكد من أن قاعدة البيانات موجودة
}

// تكوين أنابيب HTTP (التعامل مع الاستثناءات، HSTS، إلخ)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); // إضافة خدمة المصادقة

// ضبط المسارات (Routing)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
