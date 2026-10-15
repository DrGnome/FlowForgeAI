
namespace FlowForge.Application.Workflows;

public interface IWorkflowStep
{
    string Name { get; }

    Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default);
}
