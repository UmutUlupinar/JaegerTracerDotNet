using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Shared.Http;

public interface IHttpClientService
{
    Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken cancellationToken = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string url, CancellationToken cancellationToken = default);
}

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientService> _logger;
    private static readonly ActivitySource ActivitySource = new("HttpClientService");

    public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"HTTP GET {url}");
        activity?.SetTag("http.method", "GET");
        activity?.SetTag("http.url", url);

        try
        {
            _logger.LogInformation("Sending GET request to {Url}", url);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("GET request to {Url} completed successfully", url);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "HTTP GET request failed for {Url}", url);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"HTTP POST {url}");
        activity?.SetTag("http.method", "POST");
        activity?.SetTag("http.url", url);

        try
        {
            _logger.LogInformation("Sending POST request to {Url}", url);
            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("POST request to {Url} completed successfully", url);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "HTTP POST request failed for {Url}", url);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"HTTP PUT {url}");
        activity?.SetTag("http.method", "PUT");
        activity?.SetTag("http.url", url);

        try
        {
            _logger.LogInformation("Sending PUT request to {Url}", url);
            var response = await _httpClient.PutAsJsonAsync(url, request, cancellationToken);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            _logger.LogInformation("PUT request to {Url} completed successfully", url);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "HTTP PUT request failed for {Url}", url);
            throw;
        }
    }

    public async Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity($"HTTP DELETE {url}");
        activity?.SetTag("http.method", "DELETE");
        activity?.SetTag("http.url", url);

        try
        {
            _logger.LogInformation("Sending DELETE request to {Url}", url);
            var response = await _httpClient.DeleteAsync(url, cancellationToken);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("DELETE request to {Url} completed successfully", url);
        }
        catch (HttpRequestException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "HTTP DELETE request failed for {Url}", url);
            throw;
        }
    }
}

