
using FlowForge.Application;
using FlowForge.Application.Workflows;

namespace FlowForge.UnitTests;

public class WorkflowFailureTests
{
    [Fact]
    public async Task RunAsync_WhenStepFails_RecordsFailureAndStops()
    {
        var steps = new IWorkflowStep[]
        {
            new SuccessfulStep(),
            new FailingStep(),
            new SuccessfulStep()
        };

        var runner = new WorkflowRunner(steps);

        var result = await runner.RunAsync("Test workflow");

        Assert.Equal("Failed", result.Status);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal("Succeeded", result.Steps[0].Status);
        Assert.Equal("Failed", result.Steps[1].Status);
    }

    [Fact]
    public async Task RunAsync_WhenGoalIsEmpty_ReturnsFailedRun()
    {
        var runner = new WorkflowRunner(
            new IWorkflowStep[]
            {
                new ValidateGoalStep()
            });

        var result = await runner.RunAsync(" ");

        Assert.Equal("Failed", result.Status);
        Assert.Single(result.Steps);
        Assert.Equal("Failed", result.Steps[0].Status);
    }

    private sealed class SuccessfulStep : IWorkflowStep
    {
        public string Name => "Successful step";

        public Task<string> ExecuteAsync(
            string goal,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("OK");
        }
    }

    private sealed class FailingStep : IWorkflowStep
    {
        public string Name => "Failing step";

        public Task<string> ExecuteAsync(
            string goal,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated failure");
        }
    }
}
