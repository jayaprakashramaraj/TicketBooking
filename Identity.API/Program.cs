using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Identity.API.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection String
var sqlConnection = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(sqlConnection));

// JWT Configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["JwtSecret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharsLong!";
var key = Encoding.ASCII.GetBytes(jwtSecret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// CORS for React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (Environment.GetEnvironmentVariable("DISABLE_CORS") != "true")
{
    app.UseCors("AllowAll");
}

// Retry logic for database initialization
for (int i = 0; i < 10; i++)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            db.Database.EnsureCreated();
            Console.WriteLine("Database connected and created successfully.");
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}. Retrying in 5s... ({i + 1}/10)");
        Thread.Sleep(5000);
        if (i == 9) throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
