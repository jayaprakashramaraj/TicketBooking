using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketBooking.Common.Events;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ Configuration
var rabbitMQHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMQHost"]!;
var rabbitMQPortStr = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? builder.Configuration["RabbitMQPort"];
ushort.TryParse(rabbitMQPortStr, out var rabbitMQPort);
var rabbitMQUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? builder.Configuration["RabbitMQUser"]!;
var rabbitMQPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? builder.Configuration["RabbitMQPass"]!;
var rabbitConnectionString = $"amqp://{rabbitMQUser}:{rabbitMQPass}@{rabbitMQHost}:{rabbitMQPort}";

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMQHost, rabbitMQPort, "/", h => {
            h.Username(rabbitMQUser);
            h.Password(rabbitMQPass);
        });
    });
});
// Ensure the application waits for RabbitMQ to be fully connected before starting.
// This prevents silent message loss if the simulator attempts to publish before the broker is ready.
builder.Services.AddOptions<MassTransitHostOptions>().Configure(options => options.WaitUntilStarted = true);

builder.Services.AddHealthChecks()
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory() { Uri = new Uri(rabbitConnectionString) };
        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
    }, name: "rabbitmq");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAll");
app.UseFileServer();

app.MapGet("/", (HttpContext context) => 
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapFallbackToFile("index.html");

app.MapGet("/ping", () => Results.Ok(new { status = "alive", time = DateTime.UtcNow }));

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

// Endpoint to process payment result
app.MapPost("/api/payment/complete", async (PaymentResultRequest request, IBus bus) => 
{
    if (request.Status == "Success")
    {
        await bus.Publish(new PaymentCompleted
        {
            BookingId = request.BookingId,
            TransactionId = "SIM-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
            PaymentDate = DateTime.UtcNow
        });
    }
    else
    {
        await bus.Publish(new PaymentFailed
        {
            BookingId = request.BookingId,
            Reason = request.Status == "Cancel" ? "User cancelled payment" : "Simulator: Payment Failed"
        });
    }

    return Results.Ok(new { message = "Status published" });
});

app.Run();

public record PaymentResultRequest(Guid BookingId, string Status);
