using CarRentalMVC.Data;
using CarRentalMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// إعداد الاتصال بقاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// إعداد هوية المستخدم (Identity)
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// إضافة MVC و Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register background service for booking status updates
builder.Services.AddHostedService<CarRentalMVC.Services.BookingStatusBackgroundService>();

// Register IEmailSender based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, CarRentalMVC.Services.EmailSender>();
}
else
{
    builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, CarRentalMVC.Services.SendGridEmailSender>();
}

var app = builder.Build();

// Log SendGrid config issues at startup (production only)
if (!app.Environment.IsDevelopment())
{
    var config = app.Services.GetRequiredService<IConfiguration>();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var apiKey = config["SendGrid:ApiKey"];
    var senderEmail = config["SendGrid:SenderEmail"];
    var senderName = config["SendGrid:SenderName"];
    if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderName))
    {
        logger.LogError("SendGrid configuration is missing or incomplete. ApiKey, SenderEmail, and SenderName must all be set in appsettings.json.");
    }
}

// تنفيذ التهيئة عند بدء التطبيق (تكوين قاعدة البيانات وإنشاء الأدوار)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate(); // ينفذ Migrations

        // Ensure the ProfileImageUrl column exists in case migrations and the database got out of sync.
        try
        {
            var conn = dbContext.Database.GetDbConnection();
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = 'ProfileImageUrl'";
                var obj = cmd.ExecuteScalar();
                int colCount = obj != null ? Convert.ToInt32(obj) : 0;
                if (colCount == 0)
                {
                    cmd.CommandText = "ALTER TABLE AspNetUsers ADD ProfileImageUrl nvarchar(256) NULL";
                    cmd.ExecuteNonQuery();
                }
            }
            conn.Close();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not ensure ProfileImageUrl column exists; continuing startup.");
        }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        // إنشاء الأدوار الأساسية إذا لم تكن موجودة
        string[] roles = { "Admin", "Owner", "Renter" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // إنشاء مستخدم Admin إذا لم يكن موجودًا وتعيينه إلى دور Admin
        string adminEmail = "YOUSSEF@GMAIL.COM";
        string adminPassword = "Admin@123"; // استخدم كلمة مرور قوية وفكر في نقلها إلى الإعدادات
        string roleName = "Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Name = "Admin", // If still required
                FirstName = "Youssef",   // Set required FirstName
                LastName = "Admin"       // Set required LastName
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, roleName);
            }
            else
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // تأكد من أن المستخدم في دور Admin
            if (!await userManager.IsInRoleAsync(adminUser, roleName))
            {
                await userManager.AddToRoleAsync(adminUser, roleName);
            }
        }

        // Add sample cars if none exist
        if (!dbContext.Cars.Any())
        {
            var sampleCars = new List<Car>
            {
                new Car
                {
                    Brand = "Toyota",
                    Model = "Corolla",
                    Year = 2022,
                    PricePerDay = 50,
                    Location = "Cairo",
                    OwnerId = adminUser.Id,
                    Owner = adminUser,
                    AvailabilityStatus = Car.CarAvailabilityStatus.Available,
                    Bookings = new List<Booking>()
                },
                new Car
                {
                    Brand = "Hyundai",
                    Model = "Elantra",
                    Year = 2021,
                    PricePerDay = 45,
                    Location = "Giza",
                    OwnerId = adminUser.Id,
                    Owner = adminUser,
                    AvailabilityStatus = Car.CarAvailabilityStatus.Available,
                    Bookings = new List<Booking>()
                },
                new Car
                {
                    Brand = "BMW",
                    Model = "X5",
                    Year = 2023,
                    PricePerDay = 120,
                    Location = "Alexandria",
                    OwnerId = adminUser.Id,
                    Owner = adminUser,
                    AvailabilityStatus = Car.CarAvailabilityStatus.Available,
                    Bookings = new List<Booking>()
                }
            };

            dbContext.Cars.AddRange(sampleCars);
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// إعداد الـ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Ensure a default profile image exists so requests to /images/profiles/default.png don't 404.
// This writes a tiny 1x1 PNG placeholder if the file is missing.
try
{
    var defaultImageDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
    var defaultImagePath = Path.Combine(defaultImageDir, "default.png");
    if (!Directory.Exists(defaultImageDir)) Directory.CreateDirectory(defaultImageDir);
    if (!System.IO.File.Exists(defaultImagePath))
    {
        // 1x1 transparent PNG
        var base64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";
        var bytes = Convert.FromBase64String(base64);
        System.IO.File.WriteAllBytes(defaultImagePath, bytes);
    }
}
catch { }

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // يجب وضعه قبل UseAuthorization
app.UseAuthorization();

// إعداد المسارات
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
