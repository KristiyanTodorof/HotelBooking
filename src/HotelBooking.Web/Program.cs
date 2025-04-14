using HotelBooking.Application.Contracts;
using HotelBooking.Application.Mapping;
using HotelBooking.Application.Services;
using HotelBooking.Domain.Models;
using HotelBooking.Infrastructure.Data;
using HotelBooking.Infrastructure.Model_Binders;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.Infrastructure.Repositories.Interfaces;
using HotelBooking.Infrastructure.Repositories.UnitOfWork;
using HotelBooking.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

builder.Services.AddScoped<IBookingService,BookingService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISalesChannelService, SalesChannelService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddAutoMapperConfiguration();

builder.Services.AddControllers().AddCustomModelBinders();

// Add API explorer
builder.Services.AddEndpointsApiExplorer();

// Add authorization policies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Apply migrations and seed data in development
    await app.MigrateDatabaseAsync();
    await app.SeedDatabaseAsync();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    // Only apply migrations in production (no seeding)
    await app.MigrateDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

    var seeder = new DatabaseSeeder(context, userManager, roleManager, logger);
    await seeder.SeedAsync();
}

app.Run();
