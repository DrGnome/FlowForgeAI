
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
        var status = "Succeeded";

        foreach (var step in _steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                status = "Cancelled";
                break;
            }

            try
            {
                var output = await step.ExecuteAsync(
                    goal,
                    cancellationToken);

                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Succeeded",
                    output,
                    DateTimeOffset.UtcNow));
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Cancelled",
                    "Step execution was cancelled.",
                    DateTimeOffset.UtcNow));

                status = "Cancelled";
                break;
            }
            catch (Exception)
            {
                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Failed",
                    "Step execution failed. See application logs for details.",
                    DateTimeOffset.UtcNow));

                status = "Failed";
                break;
            }
        }

        return new WorkflowRun(
            Guid.NewGuid(),
            goal,
            status,
            startedAt,
            DateTimeOffset.UtcNow,
            results);
    }
}
