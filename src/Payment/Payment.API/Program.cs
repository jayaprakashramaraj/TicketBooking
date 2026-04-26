using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Payment.Infrastructure.Data;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Repositories;
using Payment.Application.Interfaces;
using Payment.Application.Services;
using Payment.API.Consumers;
using System;
using System.Threading;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection Strings
var sqlConnection = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection")!;
var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMQHost"]!;
var rabbitMQPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? builder.Configuration["RabbitMQPort"];
ushort? rabbitMQPort = ushort.TryParse(rabbitMQPortStr, out var portValue) ? portValue : null;

var rabbitMQUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? builder.Configuration["RabbitMQUser"]!;
var rabbitMQPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? builder.Configuration["RabbitMQPass"]!;

var rabbitConnectionString = rabbitMQPort.HasValue
    ? $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}:{rabbitMQPort}"
    : $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}";

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(sqlConnection));

builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnection, name: "sqlserver")
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory() { Uri = new Uri(rabbitConnectionString) };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq");

// Dependency Injection
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingInitiatedConsumer>();
    x.AddConsumer<PaymentCompletedConsumer>();
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

        cfg.ReceiveEndpoint("booking-initiated-queue", e =>
        {
            e.ConfigureConsumer<BookingInitiatedConsumer>(context);
        });

        // Use unique queue names for each service to ensure events are broadcast to all
        // interested services simultaneously. Using shared names would cause RabbitMQ 
        // to distribute messages to only one of the services (Competing Consumers).
        cfg.ReceiveEndpoint("payment-service-payment-completed-queue", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(context);
        });

        cfg.ReceiveEndpoint("payment-service-payment-failed-queue", e =>
        {
            e.ConfigureConsumer<PaymentFailedConsumer>(context);
        });
    });
});

var app = builder.Build();

if (Environment.GetEnvironmentVariable("DISABLE_CORS") != "true")
{
    app.UseCors("AllowAll");
}

// Database Initialization with Retry
for (int i = 0; i < 15; i++)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            db.Database.EnsureCreated();
            Console.WriteLine("Database connected and created successfully.");
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}. Retrying in 5s... ({i + 1}/15)");        
        Thread.Sleep(5000);
        if (i == 14) throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

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
