using DataRetrievalService.Api.Mapping;
using DataRetrievalService.Api.Validation.Auth;
using DataRetrievalService.Application;
using DataRetrievalService.Application.Mapping;
using DataRetrievalService.Application.Options;
using DataRetrievalService.Infrastructure;
using DataRetrievalService.Infrastructure.Identity;
using DataRetrievalService.Infrastructure.Persistence;
using DataRetrievalService.Infrastructure.Storage;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseSqlServer(
        config.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("DataRetrievalService.Infrastructure"));
});

builder.Services.AddIdentityCore<IdentityUser>(o =>
{
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

var jwtSection = config.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    // You could throw here to force setting the key in dev:
    // throw new InvalidOperationException("Jwt:Key is not configured.");
    // For convenience we continue, but token creation will fail without a key.
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty));

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey
        };
    });

builder.Services.
    AddOptions<DataRetrievalSettings>()
    .Bind(builder.Configuration.GetSection("DataRetrieval"))
    .Validate(s => s.CacheTtlMinutes > 0 && s.FileTtlMinutes > 0,
        "CacheTtlMinutes and FileTtlMinutes must be greater than zero")
    .ValidateOnStart();

builder.Services
    .AddOptions<FileStorageSettings>()
    .Bind(builder.Configuration.GetSection("FileStorage"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Path), "FileStorage.Path is required")
    .Validate(s => s.CleanupIntervalMinutes >= 1 && s.CleanupIntervalMinutes <= 1440,
              "FileStorage.CleanupIntervalMinutes must be between 1 and 1440.")
    .ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is required")
    .ValidateOnStart();

// Bind & validate seed users (Dev only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOptions<SeedUsersOptions>()
        .Bind(builder.Configuration.GetSection("SeedUsers"))
        .Validate(o =>
            !string.IsNullOrWhiteSpace(o.AdminEmail) &&
            !string.IsNullOrWhiteSpace(o.AdminPassword) &&
            !string.IsNullOrWhiteSpace(o.UserEmail) &&
            !string.IsNullOrWhiteSpace(o.UserPassword),
            "SeedUsers: all four values (AdminEmail, AdminPassword, UserEmail, UserPassword) must be set in Development.")
        .ValidateOnStart();
}

if (config.GetValue<bool>("Redis:UseRedis"))
{
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = config["Redis:Configuration"];
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddHostedService<FileStorageCleanupService>();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

builder.Services.AddAutoMapper(
    typeof(MappingProfile).Assembly, 
    typeof(ApiMappingProfile).Assembly
);

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DataRetrievalService API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddCors(opt =>
{
    var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    opt.AddPolicy("default", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.Configure<SeedUsersOptions>(config.GetSection("SeedUsers"));
builder.Services.Configure<DataRetrievalSettings>(config.GetSection("DataRetrieval"));

var app = builder.Build();

// Migrate & (Dev-only) seed
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (env.IsDevelopment())
    {
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var seedOpts = scope.ServiceProvider.GetRequiredService<IOptions<SeedUsersOptions>>();
        await IdentitySeeder.SeedAsync(roles, users, seedOpts.Value);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

if (!builder.Configuration.GetValue<bool>("DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.UseCors("default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
