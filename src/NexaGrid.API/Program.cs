using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using NexaGrid.API.Data;
using NexaGrid.API.Repositories;
using NexaGrid.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure controllers and readable enum values.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// Configure the NexaGrid SQL Server database.
builder.Services.AddDbContext<NexaGridDbContext>(options =>
{
    string connectionString =
        builder.Configuration.GetConnectionString(
            "NexaGridDatabase")
        ?? throw new InvalidOperationException(
            "The NexaGrid database connection string is missing.");

    options.UseSqlServer(connectionString);
});

// Register sensor functionality.
builder.Services.AddScoped<
    ISensorRepository,
    SensorRepository>();

builder.Services.AddScoped<
    ISensorService,
    SensorService>();

// Register telemetry functionality.
builder.Services.AddScoped<
    ITelemetryRepository,
    TelemetryRepository>();

builder.Services.AddScoped<
    ITelemetryService,
    TelemetryService>();

// Register sensor-attachment functionality.
builder.Services.AddScoped<
    IAttachmentRepository,
    AttachmentRepository>();

builder.Services.AddScoped<
    IAttachmentService,
    AttachmentService>();

// Limit multipart uploads to 10 MB.
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        10 * 1024 * 1024;
});

// Permit the Windows Forms client to access the API.
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

// Register OpenAPI documentation.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("NexaGridClient");

app.MapControllers();

// General API health endpoint.
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

// Required by the API integration-test project.
public partial class Program
{
}