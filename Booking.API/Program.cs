using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Booking.API.Consumers;
using Booking.API.Data;
using Booking.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection")!;
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var rabbitMQHost = builder.Configuration["RabbitMQHost"] ?? "localhost";

builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnection, name: "sqlserver")
    .AddRedis(redisConnection, name: "redis")
    .AddRabbitMQ(sp => 
    {
        var factory = new ConnectionFactory() 
        { 
            Uri = new Uri($"amqp://guest:guest@{rabbitMQHost}:5672")
        };
        // Use Task.Run to ensure we don't block the thread in a way that causes deadlocks in some sync contexts
        return Task.Run(() => factory.CreateConnectionAsync()).GetAwaiter().GetResult();
    }, name: "rabbitmq");

// Redis Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddScoped<ISeatReservationService, RedisSeatReservationService>();

builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
        var rabbitMQHost = builder.Configuration["RabbitMQHost"] ?? "localhost";
        cfg.Host(rabbitMQHost, "/");
        
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

var app = builder.Build();

app.UseCors("AllowAll");

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
