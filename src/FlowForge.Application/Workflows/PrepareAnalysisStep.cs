
namespace FlowForge.Application.Workflows;

public sealed class PrepareAnalysisStep : IWorkflowStep
{
    public string Name => "Prepare analysis";

    public Task<WorkflowStepOutput> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new WorkflowStepOutput(
            $"Prepared the goal for processing: {input.Goal}"));
    }
}
