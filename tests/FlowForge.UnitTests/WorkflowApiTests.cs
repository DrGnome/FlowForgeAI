using System.Net;
using System.Net.Http.Json;
using FlowForge.Application.AI;
using FlowForge.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FlowForge.UnitTests;

public sealed class WorkflowApiTests
{
    [Fact]
    public async Task RunWorkflow_ValidGoal_ReturnsOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new { goal = "Analyse a document" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_EmptyGoal_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new { goal = "" });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task RunWorkflow_AiFailure_ReturnsBadGateway()
    {
        using var factory = CreateFactory(
            simulateFailure: true);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/workflows/run",
            new { goal = "Analyse a document" });

        Assert.Equal(
            HttpStatusCode.BadGateway,
            response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "The workflow could not be completed.",
            body);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        bool simulateFailure = false)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAiService>();

                    services.AddSingleton<IAiService>(
                        new FakeAiService(simulateFailure));
                });
            });
    }

    private sealed class FakeAiService(
        bool simulateFailure) : IAiService
    {
        public Task<AnalysisResult> AnalyzeAsync(
            string goal,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (simulateFailure)
            {
                throw new HttpRequestException(
                    "Simulated local AI service failure.");
            }

            return Task.FromResult(
                new AnalysisResult(
                    "A test summary",
                    ["A test finding"],
                    ["A test caveat"]));
        }
    }
}