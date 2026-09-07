using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Services;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(
        string baseAddress = "http://localhost:5211")
    {
        _httpClient =
            new HttpClient
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromSeconds(120)
            };

        _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        _jsonOptions.Converters.Add(
            new JsonStringEnumConverter());
    }

    public HttpClient HttpClient => _httpClient;

    public async Task<bool> IsHealthyAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    "/api/health",
                    cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<T?> GetAsync<T>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                endpoint,
                cancellationToken);

        return await ReadResponseAsync<T>(
            response,
            cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                endpoint,
                request,
                _jsonOptions,
                cancellationToken);

        return await ReadResponseAsync<TResponse>(
            response,
            cancellationToken);
    }

    public async Task<TResponse?> PostMultipartAsync<TResponse>(
        string endpoint,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.PostAsync(
                endpoint,
                content,
                cancellationToken);

        return await ReadResponseAsync<TResponse>(
            response,
            cancellationToken);
    }

    public async Task DownloadFileAsync(
        string endpoint,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                endpoint,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string responseText =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new ApiException(
                ExtractErrorMessage(responseText),
                (int)response.StatusCode);
        }

        await using Stream responseStream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        await using var outputStream =
            new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81_920,
                useAsync: true);

        await responseStream.CopyToAsync(
            outputStream,
            cancellationToken);
    }

    private async Task<T?> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(
                ExtractErrorMessage(responseText),
                (int)response.StatusCode);
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            responseText,
            _jsonOptions);
    }

    private static string ExtractErrorMessage(
        string responseText)
    {
        try
        {
            using JsonDocument document =
                JsonDocument.Parse(responseText);

            if (document.RootElement.TryGetProperty(
                    "message",
                    out JsonElement message))
            {
                return message.GetString()
                    ?? "The request failed.";
            }

            if (document.RootElement.TryGetProperty(
                    "errors",
                    out JsonElement errors))
            {
                return FormatValidationErrors(errors);
            }
        }
        catch (JsonException)
        {
            // The API did not return JSON. Use the fallback message.
        }

        return "The API request could not be completed.";
    }

    private static string FormatValidationErrors(
        JsonElement errors)
    {
        var messages = new List<string>();

        foreach (JsonProperty property
                 in errors.EnumerateObject())
        {
            if (property.Value.ValueKind
                != JsonValueKind.Array)
            {
                continue;
            }

            foreach (JsonElement error
                     in property.Value.EnumerateArray())
            {
                string? text = error.GetString();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    messages.Add(text);
                }
            }
        }

        return messages.Count > 0
            ? string.Join(
                Environment.NewLine,
                messages)
            : "The submitted information is invalid.";
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
