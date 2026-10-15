
namespace FlowForge.Application.Workflows;

public sealed class GenerateResultStep : IWorkflowStep
{
    public string Name => "Generate result";

    public Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            "Demo workflow completed. AI processing will be added in a later milestone.");
    }
}
