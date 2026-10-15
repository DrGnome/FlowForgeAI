
namespace FlowForge.Application.Workflows;

public sealed class PrepareAnalysisStep : IWorkflowStep
{
    public string Name => "Prepare analysis";

    public Task<string> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            $"Prepared the goal for processing: {input.Goal}");
    }
}
