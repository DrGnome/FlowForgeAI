using System.Text.Json;
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

    public async Task<WorkflowRunResponse> RunAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;
        var results = new List<WorkflowStepResult>();
        var status = "Succeeded";
        AnalysisResult? analysis = null;

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
                    input,
                    cancellationToken);

                if (step.Name == "Generate result")
                {
                    analysis = JsonSerializer.Deserialize<AnalysisResult>(
                        output,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (analysis is null)
                    {
                        throw new InvalidOperationException(
                            "The generated analysis was empty.");
                    }
                }

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

        return new WorkflowRunResponse(
            id,
            input.Goal,
            status,
            startedAt,
            DateTimeOffset.UtcNow,
            analysis,
            results);
    }
}