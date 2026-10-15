
namespace FlowForge.Application.Workflows;

public sealed class PrepareAnalysisStep : IWorkflowStep
{
    public string Name => "Prepare analysis";

    public Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            $"Prepared the goal for processing: {goal}");
    }
}
