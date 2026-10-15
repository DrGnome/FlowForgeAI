using FlowForge.Application.AI;
using FlowForge.Application.Workflows;
using FlowForge.Domain;

namespace FlowForge.UnitTests;

public class GenerateResultStepTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsAnalysisInStepOutput()
    {
        var expected = new AnalysisResult(
            "A concise summary",
            ["Finding one"],
            ["Caveat one"]);

        var step = new GenerateResultStep(
            new FakeAiService(expected));

        var output = await step.ExecuteAsync(
            new WorkflowInput(
                "Analyse a document",
                "Document contents"));

        Assert.Equal(
            "Summary: A concise summary" + Environment.NewLine +
            "Key findings: 1" + Environment.NewLine +
            "Caveats: 1",
            output.Output);

        Assert.Equal(expected, output.Analysis);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var fake = new FakeAiService(
            new AnalysisResult("Summary", [], []));

        var step = new GenerateResultStep(fake);

        await step.ExecuteAsync(
            new WorkflowInput("Analyse", "Document contents"),
            cts.Token);

        Assert.Equal(cts.Token, fake.ReceivedToken);
    }

    private sealed class FakeAiService(
        AnalysisResult result) : IAiService
    {
        public CancellationToken ReceivedToken { get; private set; }

        public Task<AnalysisResult> AnalyzeAsync(
            string goal,
            string documentText,
            CancellationToken cancellationToken = default)
        {
            ReceivedToken = cancellationToken;
            return Task.FromResult(result);
        }
    }
}