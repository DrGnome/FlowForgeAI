
using FlowForge.Domain;

namespace FlowForge.Application.WorkflowHistory;

public interface IWorkflowRunRepository
{
    Task SaveAsync(
        WorkflowRunResponse workflowRun,
        CancellationToken cancellationToken = default);

    Task<WorkflowRunResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
