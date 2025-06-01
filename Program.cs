using EduSync.Data;
using EduSync.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// 🔧 Add Services to the Container
// ------------------------------

// ✅ Controllers + JSON cycle prevention (important for EF Core nav properties)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ✅ Swagger (only in development by default)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Database Context (SQL Server)
var config = builder.Configuration;

// 🌐 Check connection string
var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ ERROR: 'DefaultConnection' is NULL or empty!");
    throw new Exception("❌ Azure failed to load 'DefaultConnection'. Check App Service > Configuration > Connection Strings.");
}
else
{
    Console.WriteLine("✅ Connection string loaded.");
}

// 🌐 Check JWT
Console.WriteLine("✅ JWT Config Check: ");
Console.WriteLine("Issuer: " + config["Jwt:Issuer"]);
Console.WriteLine("Audience: " + config["Jwt:Audience"]);
Console.WriteLine("Key Present: " + (!string.IsNullOrEmpty(config["Jwt:Key"])));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// ✅ CORS (allow React frontend on localhost)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Dependency Injection for Custom Services
builder.Services.AddScoped<ITokenService, TokenService>();


// ✅ JWT Authentication Setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
        };
    });


// ✅ Authorization (based on [Authorize] and roles)
builder.Services.AddAuthorization();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);
// ------------------------------
// 🚀 Build and Configure HTTP Pipeline
// ------------------------------
var app = builder.Build();

// ✅ Swagger (only enabled in development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Middleware pipeline
app.UseHttpsRedirection();

// ✅ Enable CORS (before auth!)
app.UseCors("AllowReactApp");

app.UseAuthentication(); // always before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
