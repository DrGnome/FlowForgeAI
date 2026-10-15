using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FlowForge.Application;
using FlowForge.Application.AI;
using FlowForge.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FlowForge.UnitTests;

public sealed class WorkflowApiTests
{
    [Fact]
    public async Task RunWorkflow_ValidRequest_ReturnsStructuredAnalysis()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            goal = "Summarise the document.",
            documentText = "FlowForge analyses documents locally."
        };

        using var response = await client.PostAsJsonAsync(
            "/workflows/run", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal("Succeeded", root.GetProperty("status").GetString());

        var analysis = root.GetProperty("analysis");

        Assert.Equal(
            "The document describes local AI analysis.",
            analysis.GetProperty("summary").GetString());

        Assert.Equal(
            2,
            analysis.GetProperty("keyFindings").GetArrayLength());

        Assert.Single(
            analysis.GetProperty("caveats").EnumerateArray());

        Assert.NotEmpty(root.GetProperty("steps").EnumerateArray());
    }

    [Fact]
    public async Task RunWorkflow_EmptyGoal_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new { goal = "", documentText = "Some document text." });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_EmptyDocument_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new { goal = "Summarise this.", documentText = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_AiFailure_ReturnsBadGateway()
    {
        using var factory = CreateFactory(aiShouldFail: true);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new
            {
                goal = "Summarise the document.",
                documentText = "Some document text."
            });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task Health_OllamaAvailable_ReturnsHealthy()
    {
        using var factory = CreateFactory(aiAvailable: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Equal("Healthy", root.GetProperty("api").GetString());
        Assert.Equal(
            "Available",
            root.GetProperty("aiProvider").GetString());
    }

    [Fact]
    public async Task Health_OllamaUnavailable_ReturnsServiceUnavailable()
    {
        using var factory = CreateFactory(aiAvailable: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal("Degraded", root.GetProperty("status").GetString());
        Assert.Equal("Healthy", root.GetProperty("api").GetString());
        Assert.Equal(
            "Unavailable",
            root.GetProperty("aiProvider").GetString());
    }

    private static TestApiFactory CreateFactory(
        bool aiShouldFail = false,
        bool aiAvailable = true)
    {
        return new TestApiFactory(
            new FakeAiService(aiShouldFail),
            new FakeAiHealthCheck(aiAvailable));
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        private readonly IAiService _fakeAiService;
        private readonly IAiHealthCheck _fakeAiHealthCheck;

        public TestApiFactory(
            IAiService fakeAiService,
            IAiHealthCheck fakeAiHealthCheck)
        {
            _fakeAiService = fakeAiService;
            _fakeAiHealthCheck = fakeAiHealthCheck;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiService>();
                services.AddSingleton(_fakeAiService);

                services.RemoveAll<IAiHealthCheck>();
                services.AddSingleton(_fakeAiHealthCheck);
            });
        }
    }

    private sealed class FakeAiService(bool shouldFail) : IAiService
    {
        public Task<AnalysisResult> AnalyzeAsync(
            string goal,
            string documentText,
            CancellationToken cancellationToken = default)
        {
            if (shouldFail)
            {
                throw new InvalidOperationException(
                    "Simulated AI service failure.");
            }

            return Task.FromResult(
                new AnalysisResult(
                    "The document describes local AI analysis.",
                    [
                        "Analysis runs locally.",
                        "Results include structured findings."
                    ],
                    [
                        "Output quality depends on the model."
                    ]));
        }
    }

    private sealed class FakeAiHealthCheck(bool isAvailable)
        : IAiHealthCheck
    {
        public Task<bool> IsAvailableAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(isAvailable);
        }
    }
}