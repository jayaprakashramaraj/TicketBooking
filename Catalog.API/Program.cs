using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Catalog.API.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Fix MongoDB Guid Serialization for Driver 3.x
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Configuration
var mongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? builder.Configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(mongoConnection));
builder.Services.AddScoped(sp => 
    sp.GetRequiredService<IMongoClient>().GetDatabase("CatalogDb"));

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
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    var collection = db.GetCollection<Show>("Shows");
    if (!collection.AsQueryable().Any())
    {
        var shows = new List<Show>
        {
            new Show { Id = Guid.NewGuid(), MovieName = "Inception", TheaterName = "Grand Cinema", StartTime = DateTime.Today.AddHours(18), Price = 15.0m },
            new Show { Id = Guid.NewGuid(), MovieName = "The Dark Knight", TheaterName = "Grand Cinema", StartTime = DateTime.Today.AddHours(21), Price = 18.0m },
            new Show { Id = Guid.NewGuid(), MovieName = "Interstellar", TheaterName = "Star Plex", StartTime = DateTime.Today.AddHours(19), Price = 16.0m },
            new Show { Id = Guid.NewGuid(), MovieName = "Tenet", TheaterName = "IMAX Center", StartTime = DateTime.Today.AddHours(20), Price = 20.0m }
        };
        collection.InsertMany(shows);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
