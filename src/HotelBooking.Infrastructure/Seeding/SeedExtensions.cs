using HotelBooking.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Seeding
{
    /// <summary>
    /// Seeds the database with initial data when the application starts
    /// </summary>
    public static class SeedExtensions
    {
        public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

                try
                {
                    logger.LogInformation("Starting database seeding");

                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<Domain.Models.ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<Domain.Models.ApplicationRole>>();

                    var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
                    await seeder.SeedAsync();

                    logger.LogInformation("Database seeding completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database");
                }
            }

            return app;
        }

        /// <summary>
        /// Applies any pending migrations and seeds the database in development environments
        /// </summary>
        public static async Task<IApplicationBuilder> MigrateDatabaseAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

                try
                {
                    logger.LogInformation("Starting database migration");

                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();

                    logger.LogInformation("Database migration completed");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database");
                }
            }

            return app;
        }
    }
}
