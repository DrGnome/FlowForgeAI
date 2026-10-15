namespace FlowForge.Application.Workflows;

public interface IWorkflowStep
{
    string Name { get; }

    Task<WorkflowStepOutput> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default);
}