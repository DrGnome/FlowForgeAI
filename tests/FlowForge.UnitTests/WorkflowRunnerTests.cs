using FlowForge.Application;
using FlowForge.Application.Workflows;
using FlowForge.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FlowForge.Infrastructure.WorkflowHistory;

namespace FlowForge.UnitTests;

public sealed class WorkflowRunnerTests
{
    [Fact]
    public async Task RunAsync_StepsSucceed_RunsThemInOrder()
    {
        var executionOrder = new List<string>();

        var steps = new IWorkflowStep[]
        {
            new RecordingStep("First", executionOrder),
            new RecordingStep("Second", executionOrder),
            new RecordingStep("Third", executionOrder)
        };

        var runner = CreateRunner(steps);

        var result = await runner.RunAsync(
            new WorkflowInput("Test workflow", "Test document"));

        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(
            ["First", "Second", "Third"],
            executionOrder);
        Assert.Equal(3, result.Steps.Count);
        Assert.All(
            result.Steps,
            step => Assert.Equal("Succeeded", step.Status));
    }

    [Fact]
    public async Task RunAsync_StepFails_StopsAndReturnsSafeFailure()
    {
        var executionOrder = new List<string>();

        var steps = new IWorkflowStep[]
        {
            new RecordingStep("First", executionOrder),
            new RecordingStep(
                "Failing",
                executionOrder,
                shouldFail: true),
            new RecordingStep("NeverRuns", executionOrder)
        };

        var runner = CreateRunner(steps);

        var result = await runner.RunAsync(
            new WorkflowInput("Test workflow", "Test document"));

        Assert.Equal("Failed", result.Status);
        Assert.Equal(["First", "Failing"], executionOrder);
        Assert.Equal(2, result.Steps.Count);

        var failedStep = result.Steps[1];

        Assert.Equal("Failing", failedStep.Name);
        Assert.Equal("Failed", failedStep.Status);
        Assert.DoesNotContain(
            "secret",
            failedStep.Output,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_CancellationRequestedBeforeStart_ReturnsCancelled()
    {
        var executionOrder = new List<string>();
        var runner = CreateRunner(
        [
            new RecordingStep("First", executionOrder)
        ]);

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await runner.RunAsync(
            new WorkflowInput("Test workflow", "Test document"),
            cancellation.Token);

        Assert.Equal("Cancelled", result.Status);
        Assert.Empty(result.Steps);
        Assert.Empty(executionOrder);
    }

    [Fact]
    public async Task RunAsync_StepReturnsAnalysis_PreservesAnalysis()
    {
        var expectedAnalysis = new AnalysisResult(
            "Test summary",
            ["Finding one", "Finding two"],
            ["Test caveat"]);

        var runner = CreateRunner(
        [
            new RecordingStep(
                "Analysis",
                [],
                analysis: expectedAnalysis)
        ]);

        var result = await runner.RunAsync(
            new WorkflowInput("Test workflow", "Test document"));

        Assert.Equal("Succeeded", result.Status);
        Assert.NotNull(result.Analysis);
        Assert.Equal("Test summary", result.Analysis.Summary);
        Assert.Equal(
            ["Finding one", "Finding two"],
            result.Analysis.KeyFindings);
        Assert.Equal(
            ["Test caveat"],
            result.Analysis.Caveats);
    }

    [Fact]
public async Task RunAsync_CancelledDuringStep_StopsAndMarksWorkflowCancelled()
{
    using var cancellation = new CancellationTokenSource();
    var stepStarted = new TaskCompletionSource(
        TaskCreationOptions.RunContinuationsAsynchronously);
    var executionOrder = new List<string>();

    var steps = new IWorkflowStep[]
    {
        new BlockingStep(stepStarted, executionOrder),
        new RecordingStep("NeverRuns", executionOrder)
    };

    var runner = CreateRunner(steps);

    var runTask = runner.RunAsync(
        new WorkflowInput("Test workflow", "Test document"),
        cancellation.Token);

    // Wait until the first step is actively executing.
    await stepStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

    // Cancel while that step is running.
    await cancellation.CancelAsync();

    var result = await runTask.WaitAsync(TimeSpan.FromSeconds(5));

    Assert.Equal("Cancelled", result.Status);
    Assert.Equal(["Blocking"], executionOrder);

    var cancelledStep = Assert.Single(result.Steps);
    Assert.Equal("Blocking", cancelledStep.Name);
    Assert.Equal("Cancelled", cancelledStep.Status);
}


private static WorkflowRunner CreateRunner(IEnumerable<IWorkflowStep> steps) =>
    new(
        steps,
        NullLogger<WorkflowRunner>.Instance,
        new InMemoryWorkflowRunRepository());

    private sealed class BlockingStep(
    TaskCompletionSource stepStarted,
    List<string> executionOrder) : IWorkflowStep
{
    public string Name => "Blocking";

    public async Task<WorkflowStepOutput> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        executionOrder.Add(Name);

        // Signal that execution has begun.
        stepStarted.SetResult();

        // Wait until cancellation is requested.
        await Task.Delay(
            Timeout.InfiniteTimeSpan,
            cancellationToken);

        return new WorkflowStepOutput("This should never be returned.");
    }
}

    private sealed class RecordingStep(
        string name,
        List<string> executionOrder,
        bool shouldFail = false,
        AnalysisResult? analysis = null) : IWorkflowStep
    {
        public string Name => name;

        public Task<WorkflowStepOutput> ExecuteAsync(
            WorkflowInput input,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            executionOrder.Add(name);

            if (shouldFail)
            {
                throw new InvalidOperationException(
                    "A secret internal test exception.");
            }

            return Task.FromResult(
                new WorkflowStepOutput(
                    $"Output from {name}",
                    analysis));
        }
    }
}