using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Entities;
using Scientific_Journal_Publication_Trend_Tracking_System.Domain.Enums;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Persistence;
using Scientific_Journal_Publication_Trend_Tracking_System.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Scientific_Journal_Publication_Trend_Tracking_System.src.Infrastructure.Authentication
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            var adminExists = await dbContext.Users
                .AnyAsync(x => x.Role == UserRole.Admin);

            if (adminExists)
                return;

            var admin = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@journal.com",
                FullName = "System Administrator",
                Role = UserRole.Admin,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                // adjust property name to your User entity
                PasswordHash = passwordHasher.HashPassword("Admin@123")
            };

            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync();
        }
    }
}
