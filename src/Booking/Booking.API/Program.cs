using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Booking.API.Consumers;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Repositories;
using Booking.Infrastructure.Services;
using Booking.Infrastructure.MessageBrokers;
using Booking.Domain.Repositories;
using Booking.Domain.Services;
using Booking.Application.Interfaces;
using Booking.Application.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
var sqlConnection = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection")!;
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? builder.Configuration.GetConnectionString("Redis") ?? "localhost";

var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMQHost"] ?? "localhost";
var rabbitMQPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? builder.Configuration["RabbitMQPort"];
ushort? rabbitMQPort = ushort.TryParse(rabbitMQPortStr, out var portValue) ? portValue : null;

var rabbitMQUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? builder.Configuration["RabbitMQUser"] ?? "guest";
var rabbitMQPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? builder.Configuration["RabbitMQPass"] ?? "guest";

var rabbitConnectionString = rabbitMQPort.HasValue 
    ? $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}:{rabbitMQPort}"
    : $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}";

builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnection, name: "sqlserver")
    .AddRedis(redisHost, name: "redis")
    .AddRabbitMQ(sp => 
    {
        var factory = new ConnectionFactory() { Uri = new Uri(rabbitConnectionString) };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq");

// Infrastructure Configuration
IConnectionMultiplexer? multiplexer = null;
try 
{
    multiplexer = ConnectionMultiplexer.Connect(redisHost);
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not connect to Redis at {redisHost}. Error: {ex.Message}");
}

if (multiplexer != null)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}
else
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => null!); 
}

builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ISeatReservationService, RedisSeatReservationService>();
builder.Services.AddScoped<IEventBus, EventBus>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ISeatService, SeatService>();

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(sqlConnection));

// CORS for React App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedConsumer>();
    x.AddConsumer<BookingRequestedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        if (rabbitMQPort.HasValue)
        {
            cfg.Host(rabbitMQHost, rabbitMQPort.Value, "/", h => {
                h.Username(rabbitMQUser);
                h.Password(rabbitMQPass);
            });
        }
        else
        {
            cfg.Host(rabbitMQHost, "/", h => {
                h.Username(rabbitMQUser);
                h.Password(rabbitMQPass);
            });
        }
        
        cfg.ReceiveEndpoint("booking-requested-queue", e =>
        {
            e.ConfigureConsumer<BookingRequestedConsumer>(context);
        });

        cfg.ReceiveEndpoint("payment-completed-queue", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("booking-payment-failed-queue", e =>
        {
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
        });
    });
});
// Explicitly add MassTransit health checks
builder.Services.AddOptions<MassTransitHostOptions>().Configure(options => options.WaitUntilStarted = true);


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
            var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            db.Database.EnsureCreated();
            Console.WriteLine("Database connected and created successfully.");
            break;
        }
    }
    catch (Exception)
    {
        Console.WriteLine($"Database connection failed. Retrying in 5s... ({i + 1}/10)");
        Thread.Sleep(5000);
        if (i == 9) throw;
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                component = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            }),
            totalDuration = report.TotalDuration
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.MapControllers();

app.Run();
