using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using NexaGrid.Shared.DTOs;

namespace NexaGrid.Desktop.Services;
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
    public HttpClient HttpClient => _httpClient;
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
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
/*Software Engineering Stack Exchange (2015) Service layer vs Repository layer in desktop apps*/
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
