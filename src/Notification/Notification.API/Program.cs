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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis Configuration
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisHost));

// RabbitMQ Configuration
var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMQHost"] ?? "localhost";
var rabbitMQPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? builder.Configuration["RabbitMQPort"];
ushort? rabbitMQPort = ushort.TryParse(rabbitMQPortStr, out var portValue) ? portValue : null;

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
            cfg.Host(rabbitMQHost, rabbitMQPort.Value, "/", h => { });
        }
        else
        {
            cfg.Host(rabbitMQHost, "/", h => { });
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
app.MapControllers();

app.Run();
