using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

using NexaGrid.API.Data;
using NexaGrid.API.Repositories;
using NexaGrid.API.Services;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// Controllers and JSON
// =========================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());

        options.JsonSerializerOptions.PropertyNameCaseInsensitive =
            true;
    });

// =========================================================
// SQL Server database
// =========================================================

string connectionString =
    builder.Configuration.GetConnectionString(
        "NexaGridDatabase")
    ?? throw new InvalidOperationException(
        "The NexaGridDatabase connection string was not found.");

builder.Services.AddDbContext<NexaGridDbContext>(
    options =>
    {
        options.UseSqlServer(connectionString);
    });

// =========================================================
// Repository registrations
// =========================================================
/*OWASP Foundation (2026) ‘Input Validation'*/
builder.Services.AddScoped<
    ISensorRepository,
    SensorRepository>();

builder.Services.AddScoped<
    ITelemetryRepository,
    TelemetryRepository>();

builder.Services.AddScoped<
    IAttachmentRepository,
    AttachmentRepository>();

// =========================================================
// Service registrations
// =========================================================

builder.Services.AddScoped<
    ISensorService,
    SensorService>();

builder.Services.AddScoped<
    ITelemetryService,
    TelemetryService>();

builder.Services.AddScoped<
    IAttachmentService,
    AttachmentService>();

// =========================================================
// File upload configuration
// =========================================================

const long maximumUploadSize =
    10L * 1024L * 1024L;

builder.Services.Configure<FormOptions>(
    options =>
    {
        options.MultipartBodyLengthLimit =
            maximumUploadSize;
    });

// =========================================================
// OpenAPI documentation
// =========================================================

builder.Services.AddOpenApi();

var app = builder.Build();

// =========================================================
// HTTP request pipeline
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

// =========================================================
// Health endpoint
// =========================================================

app.MapGet(
        "/api/health",
        () =>
        {
            return Results.Ok(
                new
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