using System.Net.Http.Json;
using FlowForge.Application.AI;
using Microsoft.Extensions.Options;

namespace FlowForge.Infrastructure.AI;

public sealed class OllamaHealthCheck : IAiHealthCheck
{
    private readonly HttpClient _httpClient;

    public OllamaHealthCheck(
        HttpClient httpClient,
        IOptions<AiOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(3);
    }

    public async Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "api/tags",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}