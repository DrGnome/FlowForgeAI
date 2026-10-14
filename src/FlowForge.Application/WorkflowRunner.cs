
using FlowForge.Domain;

namespace FlowForge.Application;

public sealed class WorkflowRunner
{
    public Task<WorkflowRun> RunAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(goal))
        {
            throw new ArgumentException(
                "A workflow goal is required.",
                nameof(goal));
        }

        var startedAt = DateTimeOffset.UtcNow;
        var steps = new List<WorkflowStepResult>();

        steps.Add(new WorkflowStepResult(
            "Validate goal",
            "Succeeded",
            "The workflow goal is valid.",
            DateTimeOffset.UtcNow));

        steps.Add(new WorkflowStepResult(
            "Prepare analysis",
            "Succeeded",
            $"Prepared the goal for processing: {goal}",
            DateTimeOffset.UtcNow));

        steps.Add(new WorkflowStepResult(
            "Generate result",
            "Succeeded",
            "Demo workflow completed. AI processing will be added in a later milestone.",
            DateTimeOffset.UtcNow));

        var completedAt = DateTimeOffset.UtcNow;

        var run = new WorkflowRun(
            Guid.NewGuid(),
            goal,
            "Succeeded",
            startedAt,
            completedAt,
            steps);

        return Task.FromResult(run);
    }
}
