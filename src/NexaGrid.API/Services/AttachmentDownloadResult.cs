namespace NexaGrid.API.Services;
/*CodeProject (2018)*/
public sealed class AttachmentDownloadResult
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public required string FileName { get; init; }
}