public static void Main(string[] args)
{
    var host = CreateHostBuilder(args).Build();

    // Set environment to Development if not already set
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (string.IsNullOrEmpty(env))
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    host.Run();
}
