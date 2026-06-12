using Microsoft.AspNetCore.Identity;
using Peer_Car.Domain.Entities;
using Peer_Car.Infrastructure.Seeders;

namespace Peer_Car.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // 1. تشغيل رولز
            await RoleSeeder.SeedAsync(roleManager);

            // 2. تشغيل يوزر
            await UserSeeder.SeedAsync(userManager);

            // 3. تشغيل عربيات (محتاجين ID الأدمن اللي لسه مكرينه)
            var admin = await userManager.FindByEmailAsync("admin@peercar.com");
            if (admin != null)
            {
                await CarSeeder.SeedAsync(context, admin.Id);
            }
        }
    }
}
