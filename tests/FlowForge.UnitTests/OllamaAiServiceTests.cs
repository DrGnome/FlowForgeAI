using System.Net;
using System.Text;
using System.Text.Json;
using FlowForge.Application.AI;
using FlowForge.Domain;
using FlowForge.Infrastructure.AI;
using Microsoft.Extensions.Options;
using Xunit;

namespace FlowForge.UnitTests;

public sealed class OllamaAiServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ValidResponse_ReturnsAnalysis()
    {
        var analysisJson = """
            {
                "summary": "AI workflows automate repeatable tasks.",
                "keyFindings": [
                    "Tasks can be automated",
                    "Results can be standardised"
                ],
                "caveats": [
                    "AI output should be verified"
                ]
            }
            """;

        using var client = CreateClient(
            CreateChatResponse(analysisJson));

        var service = CreateService(client);

        var result = await service.AnalyzeAsync(
            "Explain AI workflows",
            "AI workflows can automate repeatable tasks.");

        Assert.Equal(
            "AI workflows automate repeatable tasks.",
            result.Summary);

        Assert.Equal(
            2,
            result.KeyFindings.Count);

        Assert.Equal(
            "AI output should be verified",
            Assert.Single(result.Caveats));
    }

    [Fact]
    public async Task AnalyzeAsync_HttpError_ThrowsHttpRequestException()
    {
        using var client = CreateClient(
            """{"error":"Service unavailable"}""",
            HttpStatusCode.ServiceUnavailable);

        var service = CreateService(client);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.AnalyzeAsync(
                "Analyse a document",
                "Document contents"));
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidJson_ThrowsInvalidOperationException()
    {
        using var client = CreateClient(
            CreateChatResponse("This is not valid JSON"));

        var service = CreateService(client);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AnalyzeAsync(
                    "Analyse a document",
                    "Document contents"));

        Assert.Contains("invalid analysis JSON", exception.Message);
        Assert.IsType<JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task AnalyzeAsync_IncompleteAnalysis_ThrowsInvalidOperationException()
    {
        var incompleteJson = """
            {
                "summary": "",
                "keyFindings": [],
                "caveats": []
            }
            """;

        using var client = CreateClient(
            CreateChatResponse(incompleteJson));

        var service = CreateService(client);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AnalyzeAsync(
                    "Analyse a document",
                    "Document contents"));

        Assert.Contains("incomplete analysis", exception.Message);
    }

    [Fact]
public async Task AnalyzeAsync_CallerCancelsRequest_ThrowsOperationCanceledException()
{
    using var client = CreateClient(
        CreateChatResponse("""{"summary":"Test","keyFindings":[],"caveats":[]}"""));

    var service = CreateService(client);
    using var cancellation = new CancellationTokenSource();
    await cancellation.CancelAsync();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(
        () => service.AnalyzeAsync(
            "Analyse a document",
            "Document contents",
            cancellation.Token));
}

[Fact]
public async Task AnalyzeAsync_EmptyGoal_ThrowsArgumentException()
{
    using var client = CreateClient(
        CreateChatResponse("""{"summary":"Test","keyFindings":[],"caveats":[]}"""));

    var service = CreateService(client);

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.AnalyzeAsync(
            " ",
            "Document contents"));
}

    [Fact]
    public async Task AnalyzeAsync_EmptyResponse_ThrowsInvalidOperationException()
    {
        using var client = CreateClient(
            CreateChatResponse(""));

        var service = CreateService(client);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.AnalyzeAsync(
                    "Analyse a document",
                    "Document contents"));

        Assert.Contains("empty message", exception.Message);
    }

    private static OllamaAiService CreateService(
        HttpClient client)
    {
        var options = Options.Create(new AiOptions
        {
            BaseUrl = "http://localhost:11434/",
            Model = "llama3.2"
        });

        return new OllamaAiService(client, options);
    }

    private static HttpClient CreateClient(
        string responseBody,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseBody,
                    Encoding.UTF8,
                    "application/json")
            });

        return new HttpClient(handler);
    }

    private static string CreateChatResponse(string content)
    {
        return JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content
            }
        });
    }

    private sealed class FakeHttpMessageHandler(
        HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}