namespace FlowForge.Application.Workflows;

public interface IWorkflowStep
{
    string Name { get; }

    Task<string> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default);
}