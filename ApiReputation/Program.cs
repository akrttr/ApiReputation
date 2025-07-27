using ApiReputation.Application.Services;
using ApiReputation.Infrastructure.Contexts;
using ApiReputation.Infrastructure.Repositories;
using ApiReputation.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using AspNetCoreRateLimit;
using ApiReputation.Application.Repositories;
using ApiReputation.Application.Interfaces;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Serilog konfigürasyonu
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Konsola yaz
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Günlük logları dosyaya kaydet
    .CreateLogger();



builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ApiInfoService>();
builder.Services.AddScoped<IUnitofWork, UnitofWork>();
var corsPolicy = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000") // React uygulamasına izin ver
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



// Rate Limiting Konfigürasyonu
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimitOptions"));
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]); 
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token successfully validated for: {context.Principal.Identity.Name}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();



// IdentityModelEventSource.ShowPII = true;
// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiReputation", Version = "v1" });

    // JWT Authentication'ı Swagger'a ekliyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer",
        Description = "JWT Authorization header kullanarak giriş yapın. \nÖrnek: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{

}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(corsPolicy);

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();

app.MapControllers();

app.Run();
