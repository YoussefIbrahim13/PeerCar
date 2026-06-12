using Peer_Car.Domain.Entities;
using Peer_Car.Domain.Enums;
using Peer_Car.Infrastructure.Data;

namespace Peer_Car.Infrastructure.Seeders
{
    public static class CarSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, Guid ownerId)
        {
            if (!context.Cars.Any())
            {
                context.Cars.AddRange(new List<Car>
            {
                new Car {
                    Brand = "BMW",
                    Model = "M4",
                    Year = 2023,
                    PricePerDay = 1500,
                    Location = "Cairo",
                    OwnerId = ownerId,
                    AvailabilityStatus = CarAvailabilityStatus.Available,
                    SubmissionStatus = CarSubmissionStatus.Approved
                },
                new Car {
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2024,
                    PricePerDay = 2000,
                    Location = "New Cairo",
                    OwnerId = ownerId,
                    AvailabilityStatus = CarAvailabilityStatus.Available,
                    SubmissionStatus = CarSubmissionStatus.Approved
                }
            });
                await context.SaveChangesAsync();
            }
        }
    }
}