using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Net.Sockets;
using System.Text;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var rawKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing");

var keyBytes = Encoding.UTF8.GetBytes(rawKey);

if (keyBytes.Length < 32)
{
    var newKey = new byte[32];
    Array.Copy(keyBytes, newKey, keyBytes.Length);
    keyBytes = newKey;
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

Console.WriteLine("MY BACKEND BUILD IS RUNNING");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 20;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            logger.LogInformation("Applying migrations, attempt {Attempt}/{MaxAttempts}", attempt, maxAttempts);
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
            break;
        }
        catch (Exception ex) when (
            ex is NpgsqlException ||
            ex is SocketException ||
            ex.InnerException is NpgsqlException ||
            ex.InnerException is SocketException)
        {
            logger.LogWarning(ex, "Database not ready yet");

            if (attempt == maxAttempts)
                throw;

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();