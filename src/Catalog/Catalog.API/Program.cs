using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using Catalog.Domain.Repositories;
using Catalog.Infrastructure.Repositories;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Fix MongoDB Guid Serialization
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Configuration
// Mongo Configuration
var mongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? builder.Configuration.GetConnectionString("MongoConnection");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnection));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("CatalogDb"));

// Dependency Injection
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<IShowService, ShowService>();

// CORS
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

// Seed MongoDB
using (var scope = app.Services.CreateScope())
{
    var showService = scope.ServiceProvider.GetRequiredService<IShowService>();
    await showService.SeedDataAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
