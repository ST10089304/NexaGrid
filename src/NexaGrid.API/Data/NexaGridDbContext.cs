using Microsoft.EntityFrameworkCore;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Data;
/*(Stack Overflow Community, 2018; Stack Overflow Community, 2020).*/
public class NexaGridDbContext : DbContext
{
    public NexaGridDbContext(
        DbContextOptions<NexaGridDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<Sensor> Sensors => Set<Sensor>();

    public DbSet<TelemetryRecord> TelemetryRecords =>
        Set<TelemetryRecord>();

    public DbSet<SensorAttachment> SensorAttachments =>
        Set<SensorAttachment>();
/*(Stack Overflow Community, 2018; Stack Overflow Community, 2020).*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(device => device.Id);

            entity.Property(device => device.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(device => device.MacAddress)
                .HasMaxLength(17)
                .IsRequired();

            entity.HasIndex(device => device.MacAddress)
                .IsUnique();
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.HasKey(sensor => sensor.Id);

            entity.Property(sensor => sensor.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(sensor => sensor.UniqueIdentifier)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(sensor => sensor.DeploymentLocation)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(sensor => sensor.UniqueIdentifier)
                .IsUnique();

            entity.HasOne(sensor => sensor.Device)
                .WithMany(device => device.Sensors)
                .HasForeignKey(sensor => sensor.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasKey(record => record.Id);

            entity.Property(record => record.DataType)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(record => record.Value)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(record => record.Unit)
                .HasMaxLength(30);

            entity.HasOne(record => record.Sensor)
                .WithMany(sensor => sensor.TelemetryRecords)
                .HasForeignKey(record => record.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
/*(Stack Overflow Community, 2018; Stack Overflow Community, 2020).*/
        modelBuilder.Entity<SensorAttachment>(entity =>
        {
            entity.HasKey(attachment => attachment.Id);

            entity.Property(attachment => attachment.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(attachment => attachment.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(attachment => attachment.ContentType)
                .HasMaxLength(100);

            entity.Property(attachment => attachment.StoragePath)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne(attachment => attachment.Sensor)
                .WithMany(sensor => sensor.Attachments)
                .HasForeignKey(attachment => attachment.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}