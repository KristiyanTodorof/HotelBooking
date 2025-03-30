using HotelBooking.Domain.BaseModels;
using HotelBooking.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {

        }

        public ApplicationDbContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-O7O7SSV\\SQLEXPRESS01;Database=HotelBookingDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;");
            }
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetails> BookingDetails { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<SalesChannel> SalesChannels { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

            ConfigureRelationships(builder);
            ConfigureConstraintsAndIndexes(builder);
            ConfigureSensitiveDataProtection(builder);
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            // Configure Booking relationships
            builder.Entity<Booking>()
                .HasOne(b => b.Guest)
                .WithMany(g => g.Bookings)
                .HasForeignKey(b => b.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.BookingDetails)
                .WithOne(bd => bd.Booking)
                .HasForeignKey<Booking>(b => b.BookingDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Booking>(b => b.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Booking>()
                .HasOne(b => b.SalesChannel)
                .WithMany(sc => sc.Bookings)
                .HasForeignKey(b => b.SalesChannelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Guest relationship with ApplicationUser
            builder.Entity<Guest>()
                .HasOne(g => g.ApplicationUser)
                .WithOne(u => u.Guest)
                .HasForeignKey<Guest>(g => g.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private void ConfigureConstraintsAndIndexes(ModelBuilder builder)
        {
            // Add unique indexes
            builder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            builder.Entity<Guest>()
                .HasIndex(g => g.Email)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Add other constraints
            builder.Entity<BookingDetails>()
                .Property(bd => bd.Adults)
                .IsRequired();

            builder.Entity<Booking>()
                .Property(b => b.Hotel)
                .IsRequired();

            builder.Entity<Booking>()
                .Property(b => b.ReservationStatus)
                .HasDefaultValue("Confirmed");
        }

        private void ConfigureSensitiveDataProtection(ModelBuilder builder)
        {
            // Update to match the property name from your model
            builder.Entity<Payment>()
                .Property(p => p.CreditCard)
                .HasMaxLength(200); // Allow extra space for encrypted data
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>().ToList();
            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                if (EntityState.Added == entry.State)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (EntityState.Modified == entry.State)
                {
                    entity.ModifiedAt = DateTime.UtcNow;
                }
                else if (EntityState.Deleted == entry.State)
                {
                    entry.State = EntityState.Modified;
                    entity.DeletedAt = DateTime.UtcNow;
                    entity.IsDeleted = true;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}