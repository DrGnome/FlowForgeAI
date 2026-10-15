
using FlowForge.Application.Workflows;
using FlowForge.Domain;

namespace FlowForge.Application;

public sealed class WorkflowRunner
{
    private readonly IReadOnlyList<IWorkflowStep> _steps;

    public WorkflowRunner(IEnumerable<IWorkflowStep> steps)
    {
        _steps = steps.ToArray();
    }

    public async Task<WorkflowRun> RunAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var results = new List<WorkflowStepResult>();

        try
        {
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var output = await step.ExecuteAsync(
                    goal,
                    cancellationToken);

                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Succeeded",
                    output,
                    DateTimeOffset.UtcNow));
            }

            return new WorkflowRun(
                Guid.NewGuid(),
                goal,
                "Succeeded",
                startedAt,
                DateTimeOffset.UtcNow,
                results);
        }
        catch
        {
            // Preserve the exception so the API can report the failure.
            // Failure-state recording will be added next.
            throw;
        }
    }
}
