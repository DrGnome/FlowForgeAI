
using FlowForge.Application;
using FlowForge.Application.Workflows;
using FlowForge.Infrastructure.WorkflowHistory;
using Microsoft.Extensions.Logging.Abstractions;

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

        var runner = new WorkflowRunner(
            steps,
            NullLogger<WorkflowRunner>.Instance,
            new InMemoryWorkflowRunRepository());

        var result = await runner.RunAsync(
            new WorkflowInput("Test workflow", "Test document"));

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
            },
            NullLogger<WorkflowRunner>.Instance,
            new InMemoryWorkflowRunRepository());

        var result = await runner.RunAsync(
            new WorkflowInput(" ", "Test document"));

        Assert.Equal("Failed", result.Status);
        Assert.Single(result.Steps);
        Assert.Equal("Failed", result.Steps[0].Status);
    }

    private sealed class SuccessfulStep : IWorkflowStep
    {
        public string Name => "Successful step";

        public Task<WorkflowStepOutput> ExecuteAsync(
            WorkflowInput input,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new WorkflowStepOutput("OK"));
        }
    }

    private sealed class FailingStep : IWorkflowStep
    {
        public string Name => "Failing step";

        public Task<WorkflowStepOutput> ExecuteAsync(
            WorkflowInput input,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated failure");
        }
    }
}
