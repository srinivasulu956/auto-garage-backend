using Auto_Garage.BackgroundServices;
using Auto_Garage.Data.AutoGarageAuthDb;
using Auto_Garage.Data.AutoGarageDb;
using Auto_Garage.Middlewares;
using Auto_Garage.Models.DomainModels;
using Auto_Garage.Repositories.AdminBookingRepository;
using Auto_Garage.Repositories.AdminUserRepository;
using Auto_Garage.Repositories.BookingRepository;
using Auto_Garage.Repositories.InvoiceRepository;
using Auto_Garage.Repositories.JobWorkLogRepository;
using Auto_Garage.Repositories.ServiceTypeRepository;
using Auto_Garage.Repositories.TokenRepository;
using Auto_Garage.Repositories.VehicleRepository;
using Auto_Garage.Services.AdminBookingService;
using Auto_Garage.Services.AdminUserService;
using Auto_Garage.Services.BookingService;
using Auto_Garage.Services.InvoiceService;
using Auto_Garage.Services.JobWorkLogService;
using Auto_Garage.Services.ServiceTypeService;
using Auto_Garage.Services.VehicleService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

// Replace default logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext injection
builder.Services.AddDbContext<AutoGarageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoGarageDbConnection")));

builder.Services.AddDbContext<AutoGarageAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AutoGarageAuthDbConnection")));

// Add repositories
builder.Services.AddScoped<ITokenRepositiry, TokenRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
builder.Services.AddScoped<IAdminBookingRepository, AdminBookingRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IJobWorkLogRepository, JobWorkLogRepository>();  

// Add services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IServiceTypeService, ServiceTypeService>();
builder.Services.AddScoped<IAdminBookingService, AdminBookingService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IJobWorkLogService, JobWorkLogService>();       

// add identity services
builder.Services.AddIdentityCore<AutoGarageUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AutoGarageAuthDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

// configure identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("❌ JWT FAILED: " + context.Exception.GetType().Name);
                Console.WriteLine("❌ REASON: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(
        new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build()
    );

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:7600", "https://localhost:7600")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Background services
builder.Services.AddHostedService<TokenCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthorization();
app.MapControllers();

Log.Information("Application started successfully");

app.Run();