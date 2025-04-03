using HotelBooking.Application.Mapping;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Model_Binders;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    })
);

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Configure identity options
    options.Password.RequireDigit = builder.Configuration.GetValue<bool>("SecuritySettings:PasswordRequireDigit");
    options.Password.RequireLowercase = builder.Configuration.GetValue<bool>("SecuritySettings:PasswordRequireLowercase");
    options.Password.RequireUppercase = builder.Configuration.GetValue<bool>("SecuritySettings:PasswordRequireUppercase");
    options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue<bool>("SecuritySettings:PasswordRequireNonAlphanumeric");
    options.Password.RequiredLength = builder.Configuration.GetValue<int>("SecuritySettings:PasswordMinLength");

    options.Lockout.MaxFailedAccessAttempts = builder.Configuration.GetValue<int>("SecuritySettings:UserLockoutMaxFailedAttempts");
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("SecuritySettings:UserLockoutDurationMinutes"));

    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register infrastructure services (repositories, etc.)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingDetailsRepository, BookingDetailsRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<ISalesChannelRepository, SalesChannelRepository>();

builder.Services.AddAutoMapperConfiguration();

builder.Services.AddControllers().AddCustomModelBinders();

// Add API explorer
builder.Services.AddEndpointsApiExplorer();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageBookings", policy => policy.RequireClaim("Permission", "ManageBookings"));
    options.AddPolicy("CanManageRooms", policy => policy.RequireClaim("Permission", "ManageRooms"));
    options.AddPolicy("CanManageUsers", policy => policy.RequireClaim("Permission", "ManageUsers"));
    options.AddPolicy("CanAccessReports", policy => policy.RequireClaim("Permission", "AccessReports"));
    options.AddPolicy("CanManagePayments", policy => policy.RequireClaim("Permission", "ManagePayments"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Create database and apply migrations during development
//if (app.Environment.IsDevelopment())
//{
//    // Ensure database is created and migrations are applied
//    using (var scope = app.Services.CreateScope())
//    {
//        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//        dbContext.Database.Migrate();

//        // Add seed data if needed
//    }
//}

app.Run();
