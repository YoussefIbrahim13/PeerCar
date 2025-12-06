public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
{
    // ...existing code...

    var config = app.ApplicationServices.GetService<IConfiguration>();
    var adminEmail = config["AdminUser:Email"];
    var adminPassword = config["AdminUser:Password"];

    // Ensure admin user creation logic uses these credentials
    SeedAdminUser(serviceProvider, adminEmail, adminPassword);

    // ...existing code...
}

private void SeedAdminUser(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
{
    // ...existing code for creating admin user...
}
