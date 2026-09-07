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
                BaseAddress =
                    new Uri(baseAddress),
                Timeout =
                    TimeSpan.FromSeconds(120)
            };

        _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        _jsonOptions.Converters.Add(
            new JsonStringEnumConverter());
    }

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

    public async Task<TResponse?> PostAsync<
        TRequest,
        TResponse>(
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

    public HttpClient HttpClient =>
        _httpClient;

    private async Task<T?> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string responseText =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string message =
                ExtractErrorMessage(responseText);

            throw new ApiException(
                message,
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

    private string ExtractErrorMessage(
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
            // Use the fallback message below.
        }

        return "The API request could not be completed.";
    }

    private static string FormatValidationErrors(
        JsonElement errors)
    {
        var messages =
            new List<string>();

        foreach (JsonProperty property
                 in errors.EnumerateObject())
        {
            foreach (JsonElement error
                     in property.Value.EnumerateArray())
            {
                string? text =
                    error.GetString();

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