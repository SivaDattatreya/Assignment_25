using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReqResClient.Configuration;
using ReqResClient.Exceptions;
using ReqResClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace ReqResClient.Services
{
    public class ReqResApiClient : IReqResApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReqResApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReqResApiClient(HttpClient httpClient, IOptions<ReqResApiOptions> options, ILogger<ReqResApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var value = options.Value;
            _httpClient.BaseAddress = new Uri(value.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(value.TimeOutSeconds);

            // Add API key to default headers
            if (!string.IsNullOrEmpty(value.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-api-key", value.ApiKey);
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Making GET request to {Endpoint}", endpoint);

                HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Resource not found at {Endpoint}", endpoint);
                    throw new NotFoundException($"Resource not found at {endpoint}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API request failed with status code {StatusCode}", response.StatusCode);
                    throw new ApiException(response.StatusCode, $"API request failed with status code {response.StatusCode}");
                }

                string content = await response.Content.ReadAsStringAsync();
                try
                {
                    return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize response from {Endpoint}", endpoint);
                    throw new DeserializationException("Failed to deserialize API response", ex);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed for {Endpoint}", endpoint);
                throw new ApiException(HttpStatusCode.ServiceUnavailable, "HTTP request failed", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Request timeout for {Endpoint}", endpoint);
                throw new ApiException(HttpStatusCode.RequestTimeout, "Request timed out", ex);
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ApiException && ex is not DeserializationException)
            {
                _logger.LogError(ex, "Unexpected error occurred while making request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
}