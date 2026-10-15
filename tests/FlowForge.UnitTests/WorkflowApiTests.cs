using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
        using var factory = new TestApiFactory(
            new FakeAiService(ShouldFail: false));

        using var client = factory.CreateClient();

        var request = new
        {
            goal = "Summarise the document.",
            documentText = "FlowForge analyses documents locally."
        };

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;

        Assert.Equal(
            "Summarise the document.",
            root.GetProperty("goal").GetString());

        Assert.Equal(
            "Succeeded",
            root.GetProperty("status").GetString());

        Assert.True(root.TryGetProperty("id", out _));
        Assert.True(root.TryGetProperty("startedAt", out _));
        Assert.True(root.TryGetProperty("completedAt", out _));

        var analysis = root.GetProperty("analysis");

        Assert.Equal(
            "The document describes local AI analysis.",
            analysis.GetProperty("summary").GetString());

        var findings = analysis.GetProperty("keyFindings");

        Assert.Equal(JsonValueKind.Array, findings.ValueKind);
        Assert.Equal(2, findings.GetArrayLength());
        Assert.Equal(
            "Analysis runs locally.",
            findings[0].GetString());
        Assert.Equal(
            "Results include structured findings.",
            findings[1].GetString());

        var caveats = analysis.GetProperty("caveats");

        Assert.Equal(JsonValueKind.Array, caveats.ValueKind);
        Assert.Single(caveats.EnumerateArray());
        Assert.Equal(
            "Output quality depends on the model.",
            caveats[0].GetString());

        var steps = root.GetProperty("steps");

        Assert.Equal(JsonValueKind.Array, steps.ValueKind);
        Assert.NotEmpty(steps.EnumerateArray());

        Assert.Contains(
            steps.EnumerateArray(),
            step => step.GetProperty("name").GetString()
                    == "Generate result");
    }

    [Fact]
    public async Task RunWorkflow_EmptyGoal_ReturnsBadRequest()
    {
        using var factory = new TestApiFactory(
            new FakeAiService(ShouldFail: false));

        using var client = factory.CreateClient();

        var request = new
        {
            goal = "",
            documentText = "Some document text."
        };

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_EmptyDocument_ReturnsBadRequest()
    {
        using var factory = new TestApiFactory(
            new FakeAiService(ShouldFail: false));

        using var client = factory.CreateClient();

        var request = new
        {
            goal = "Summarise the document.",
            documentText = ""
        };

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_AiFailure_ReturnsBadGateway()
    {
        using var factory = new TestApiFactory(
            new FakeAiService(ShouldFail: true));

        using var client = factory.CreateClient();

        var request = new
        {
            goal = "Summarise the document.",
            documentText = "Some document text."
        };

        using var response = await client.PostAsJsonAsync(
            "/workflows/run",
            request);

        Assert.Equal(
            HttpStatusCode.BadGateway,
            response.StatusCode);
    }

    private sealed class TestApiFactory : WebApplicationFactory<Program>
    {
        private readonly IAiService _fakeAiService;

        public TestApiFactory(IAiService fakeAiService)
        {
            _fakeAiService = fakeAiService;
        }

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiService>();
                services.AddSingleton(_fakeAiService);
            });
        }
    }

    private sealed class FakeAiService(bool ShouldFail) : IAiService
    {
        public Task<AnalysisResult> AnalyzeAsync(
            string goal,
            string documentText,
            CancellationToken cancellationToken = default)
        {
            if (ShouldFail)
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
}