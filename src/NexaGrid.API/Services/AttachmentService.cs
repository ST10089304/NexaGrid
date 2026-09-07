using NexaGrid.API.Repositories;
using NexaGrid.Shared.DTOs;
using NexaGrid.Shared.Models;

namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public class AttachmentService : IAttachmentService
{
    private const long MaximumFileSizeBytes =
        10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<
        string,
        string> AllowedFileTypes =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".pdf"] = "application/pdf",
            [".txt"] = "text/plain",
            [".log"] = "text/plain",
            [".json"] = "application/json",
            [".xml"] = "application/xml",
            [".csv"] = "text/csv",
            [".yaml"] = "application/yaml",
            [".yml"] = "application/yaml",
            [".conf"] = "text/plain"
        };
/*CodeProject (2018)*/
    private readonly IAttachmentRepository
        _attachmentRepository;

    private readonly IWebHostEnvironment
        _environment;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IWebHostEnvironment environment)
    {
        _attachmentRepository =
            attachmentRepository;

        _environment = environment;
    }
/*CodeProject (2018)*/
    public async Task<SensorAttachmentResponse> UploadAsync(
        int sensorId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        Sensor? sensor =
            await _attachmentRepository.GetSensorAsync(
                sensorId,
                cancellationToken);

        if (sensor is null)
        {
            throw new KeyNotFoundException(
                $"Sensor with ID {sensorId} was not found.");
        }

        ValidateFile(file);

        string safeOriginalFileName =
            Path.GetFileName(file.FileName);

        string extension =
            Path.GetExtension(safeOriginalFileName)
                .ToLowerInvariant();

        string storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        string relativeDirectory =
            Path.Combine(
                "Uploads",
                "sensors",
                sensorId.ToString());

        string absoluteDirectory =
            Path.Combine(
                _environment.ContentRootPath,
                relativeDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        string absoluteFilePath =
            Path.Combine(
                absoluteDirectory,
                storedFileName);

        string relativeFilePath =
            Path.Combine(
                relativeDirectory,
                storedFileName);
/*CodeProject (2018)*/
        try
        {
            await using Stream inputStream =
                file.OpenReadStream();

            await using var outputStream =
                new FileStream(
                    absoluteFilePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81_920,
                    useAsync: true);

            await inputStream.CopyToAsync(
                outputStream,
                cancellationToken);

            var attachment =
                new SensorAttachment
                {
                    SensorId = sensorId,
                    OriginalFileName =
                        safeOriginalFileName,
                    StoredFileName =
                        storedFileName,
                    ContentType =
                        AllowedFileTypes[extension],
                    FileSizeBytes = file.Length,
                    StoragePath =
                        relativeFilePath,
                    UploadedAtUtc =
                        DateTime.UtcNow
                };

            await _attachmentRepository.AddAsync(
                attachment,
                cancellationToken);

            await _attachmentRepository.SaveChangesAsync(
                cancellationToken);

            attachment.Sensor = sensor;

            return MapToResponse(attachment);
        }
        catch
        {
            if (File.Exists(absoluteFilePath))
            {
                File.Delete(absoluteFilePath);
            }

            throw;
        }
    }
/*CodeProject (2018)*/
    public async Task<
        IReadOnlyList<SensorAttachmentResponse>>
        GetBySensorIdAsync(
            int sensorId,
            CancellationToken cancellationToken = default)
    {
        Sensor? sensor =
            await _attachmentRepository.GetSensorAsync(
                sensorId,
                cancellationToken);

        if (sensor is null)
        {
            throw new KeyNotFoundException(
                $"Sensor with ID {sensorId} was not found.");
        }

        List<SensorAttachment> attachments =
            await _attachmentRepository.GetBySensorIdAsync(
                sensorId,
                cancellationToken);

        return attachments
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<AttachmentDownloadResult> DownloadAsync(
        int attachmentId,
        CancellationToken cancellationToken = default)
    {
        SensorAttachment? attachment =
            await _attachmentRepository.GetByIdAsync(
                attachmentId,
                cancellationToken);

        if (attachment is null)
        {
            throw new KeyNotFoundException(
                $"Attachment with ID {attachmentId} was not found.");
        }

        string uploadsRoot =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads"));

        string absoluteFilePath =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    attachment.StoragePath));

        if (!absoluteFilePath.StartsWith(
                uploadsRoot,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The attachment path is invalid.");
        }

        if (!File.Exists(absoluteFilePath))
        {
            throw new FileNotFoundException(
                "The attachment file no longer exists.");
        }

        var stream =
            new FileStream(
                absoluteFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81_920,
                useAsync: true);

        return new AttachmentDownloadResult
        {
            Stream = stream,
            ContentType = attachment.ContentType,
            FileName = attachment.OriginalFileName
        };
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException(
                "Select a non-empty file to upload.");
        }

        if (file.Length > MaximumFileSizeBytes)
        {
            throw new ArgumentException(
                "The file cannot be larger than 10 MB.");
        }

        string safeFileName =
            Path.GetFileName(file.FileName);

        string extension =
            Path.GetExtension(safeFileName);

        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedFileTypes.ContainsKey(extension))
        {
            string permittedExtensions =
                string.Join(
                    ", ",
                    AllowedFileTypes.Keys
                        .OrderBy(value => value));

            throw new ArgumentException(
                $"Unsupported file type. Allowed types: {permittedExtensions}.");
        }
    }

    private static SensorAttachmentResponse MapToResponse(
        SensorAttachment attachment)
    {
        return new SensorAttachmentResponse
        {
            Id = attachment.Id,
            SensorId = attachment.SensorId,
            SensorIdentifier =
                attachment.Sensor?.UniqueIdentifier
                ?? string.Empty,
            OriginalFileName =
                attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            FormattedFileSize =
                FormatFileSize(
                    attachment.FileSizeBytes),
            UploadedAtUtc =
                attachment.UploadedAtUtc,
            DownloadUrl =
                $"/api/attachments/{attachment.Id}/download"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
        {
            return $"{bytes / 1024d / 1024d:F2} MB";
        }

        if (bytes >= 1024)
        {
            return $"{bytes / 1024d:F2} KB";
        }

        return $"{bytes} bytes";
    }
}