using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.API.Consumers;
using Notification.API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IPdfGenerator, PdfGenerator>();
builder.Services.AddSingleton<IEmailService, EmailService>();

// CORS for React App
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
        var rabbitMQHost = builder.Configuration["RabbitMQHost"] ?? "localhost";
        cfg.Host(rabbitMQHost, "/");
        cfg.ReceiveEndpoint("booking-confirmed-queue", e =>
        {
            e.ConfigureConsumer<BookingConfirmedConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
