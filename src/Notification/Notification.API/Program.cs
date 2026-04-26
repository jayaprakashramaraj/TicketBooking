using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using Notification.Application.Interfaces;
using Notification.Application.Services;
using Notification.Infrastructure.Services;
using Notification.Infrastructure.Persistence;
using Notification.Domain.Repositories;
using Notification.API.Consumers;
using System;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System.Linq;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis Configuration
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? builder.Configuration.GetConnectionString("Redis")!;

// RabbitMQ Configuration
var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMQHost"]!;
var rabbitMQPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? builder.Configuration["RabbitMQPort"];
ushort? rabbitMQPort = ushort.TryParse(rabbitMQPortStr, out var portValue) ? portValue : null;

var rabbitMQUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? builder.Configuration["RabbitMQUser"]!;
var rabbitMQPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? builder.Configuration["RabbitMQPass"]!;

var rabbitConnectionString = rabbitMQPort.HasValue
    ? $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}:{rabbitMQPort}"
    : $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}";

// Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(redisHost, name: "redis")
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory() { Uri = new Uri(rabbitConnectionString) };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq");

IConnectionMultiplexer? multiplexer = null;
try 
{
    var options = ConfigurationOptions.Parse(redisHost);
    options.ConnectTimeout = 10000;
    options.SyncTimeout = 10000;
    options.AbortOnConnectFail = false;
    multiplexer = await ConnectionMultiplexer.ConnectAsync(options);
    builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not connect to Redis at {redisHost}. Error: {ex.Message}");
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => null!);
}

// Dependency Injection
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();
builder.Services.AddScoped<ITicketRepository, RedisTicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<BookingConfirmedConsumer>();

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

        cfg.ReceiveEndpoint("booking-confirmed-queue", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
        });
    });
});

var app = builder.Build();

if (Environment.GetEnvironmentVariable("DISABLE_CORS") != "true")
{
    app.UseCors("AllowAll");
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
