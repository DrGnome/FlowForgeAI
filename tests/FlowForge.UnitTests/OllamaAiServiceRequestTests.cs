using System.Net;
using System.Text;
using System.Text.Json;
using FlowForge.Infrastructure.AI;
using Microsoft.Extensions.Options;
using Xunit;

namespace FlowForge.UnitTests;

public sealed class OllamaAiServiceRequestTests
{
    [Fact]
    public async Task AnalyzeAsync_SendsExpectedNativeApiRequest()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    CreateChatResponse(),
                    Encoding.UTF8,
                    "application/json")
            });

        using var client = new HttpClient(handler);

        var options = Options.Create(new AiOptions
        {
            BaseUrl = "http://localhost:11434/",
            Model = "llama3.2"
        });

        var service = new OllamaAiService(client, options);

        await service.AnalyzeAsync(
            "Summarise the document",
            "FlowForge uses local AI to analyse documents.");

        Assert.NotNull(handler.RequestMethod);
        Assert.Equal(HttpMethod.Post, handler.RequestMethod);
        Assert.Equal("api/chat", handler.RequestUri?.PathAndQuery.TrimStart('/'));

        Assert.NotNull(handler.RequestBody);

        using var requestJson = JsonDocument.Parse(handler.RequestBody);
        var root = requestJson.RootElement;

        Assert.Equal("llama3.2", root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal("json", root.GetProperty("format").GetString());

        var messages = root.GetProperty("messages");
        Assert.Equal(2, messages.GetArrayLength());

        Assert.Equal(
            "system",
            messages[0].GetProperty("role").GetString());

        Assert.Equal(
            "user",
            messages[1].GetProperty("role").GetString());

        var userPrompt = messages[1].GetProperty("content").GetString();

        Assert.Contains("Summarise the document", userPrompt);
        Assert.Contains(
            "FlowForge uses local AI to analyse documents.",
            userPrompt);
    }

    private static string CreateChatResponse()
    {
        return JsonSerializer.Serialize(new
        {
            message = new
            {
                role = "assistant",
                content = """
                    {
                        "summary": "FlowForge analyses documents locally.",
                        "keyFindings": ["It uses local AI."],
                        "caveats": ["Results should be verified."]
                    }
                    """
            }
        });
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public CapturingHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpMethod? RequestMethod { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestMethod = request.Method;
            RequestUri = request.RequestUri;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(
                    cancellationToken);

            return _response;
        }
    }
}