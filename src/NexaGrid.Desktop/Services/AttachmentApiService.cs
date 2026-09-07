using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Services;
/*Stack Overflow Community (2016) Proper place for business logic in WinForms applications*/
public sealed class AttachmentApiService
{
    private readonly ApiClient _apiClient;

    public AttachmentApiService(
        ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<SensorAttachmentResponse>>
        GetBySensorAsync(
            int sensorId,
            CancellationToken cancellationToken = default)
    {
        ApiResponse<List<SensorAttachmentResponse>>? response =
            await _apiClient.GetAsync<
                ApiResponse<List<SensorAttachmentResponse>>>(
                $"/api/attachments/sensor/{sensorId}",
                cancellationToken);

        return response?.Data ?? [];
    }
/*Stack Overflow Community (2016) Proper place for business logic in WinForms applications*/
    public async Task<SensorAttachmentResponse> UploadAsync(
        int sensorId,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath)
            || !File.Exists(filePath))
        {
            throw new ArgumentException(
                "Select an existing file to upload.");
        }

        await using var fileStream =
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81_920,
                useAsync: true);

        using var fileContent =
            new StreamContent(fileStream);

        using var multipartContent =
            new MultipartFormDataContent();

        multipartContent.Add(
            fileContent,
            "file",
            Path.GetFileName(filePath));

        ApiResponse<SensorAttachmentResponse>? response =
            await _apiClient.PostMultipartAsync<
                ApiResponse<SensorAttachmentResponse>>(
                $"/api/attachments/sensor/{sensorId}",
                multipartContent,
                cancellationToken);

        return response?.Data
            ?? throw new ApiException(
                "The API did not return the uploaded attachment.",
                500);
    }

    public Task DownloadAsync(
        SensorAttachmentResponse attachment,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        if (attachment.Id <= 0)
        {
            throw new ArgumentException(
                "Select a valid attachment to download.");
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException(
                "Select a download destination.");
        }

        string endpoint =
            string.IsNullOrWhiteSpace(attachment.DownloadUrl)
                ? $"/api/attachments/{attachment.Id}/download"
                : attachment.DownloadUrl;

        return _apiClient.DownloadFileAsync(
            endpoint,
            destinationPath,
            cancellationToken);
    }
}
