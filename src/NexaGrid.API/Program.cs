using Microsoft.EntityFrameworkCore;
using NexaGrid.API.Data;
using NexaGrid.API.Repositories;
using NexaGrid.API.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// Register the NexaGrid SQL Server database.
builder.Services.AddDbContext<NexaGridDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "NexaGridDatabase")));
            
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<ISensorService, SensorService>();

builder.Services.AddScoped<
    ITelemetryRepository,
    TelemetryRepository>();

builder.Services.AddScoped<
    ITelemetryService,
    TelemetryService>();

// Allow the Windows Forms application to communicate with the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("NexaGridClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Register the built-in OpenAPI document.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("NexaGridClient");

app.MapControllers();

// API health-check endpoint.
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        success = true,
        service = "NexaGrid API",
        status = "Healthy",
        version = "1.0.0",
        timestampUtc = DateTime.UtcNow
    });
})
.WithName("GetApiHealth");

app.Run();

// Required later by the integration test project.
public partial class Program
{
}