
using FlowForge.Application.AI;
using FlowForge.Application.Workflows;

namespace FlowForge.UnitTests;

public class GenerateResultStepTests
{
    [Fact]
    public async Task ExecuteAsync_UsesAiService()
    {
        var fakeAi = new FakeAiService();
        var step = new GenerateResultStep(fakeAi);

        var result = await step.ExecuteAsync(
            "Summarise a document");

        Assert.Equal(
            "Fake analysis result",
            result);

        Assert.Equal(
            "Summarise a document",
            fakeAi.ReceivedGoal);
    }

    private sealed class FakeAiService : IAiService
    {
        public string? ReceivedGoal { get; private set; }

        public Task<string> AnalyzeAsync(
            string goal,
            CancellationToken cancellationToken = default)
        {
            ReceivedGoal = goal;

            return Task.FromResult("Fake analysis result");
        }
    }
}
