
using System.Collections.Concurrent;
using FlowForge.Application.WorkflowHistory;
using FlowForge.Domain;

namespace FlowForge.Infrastructure.WorkflowHistory;

public sealed class InMemoryWorkflowRunRepository
    : IWorkflowRunRepository
{
    private readonly ConcurrentDictionary<Guid, WorkflowRunResponse> _runs
        = new();

    public Task SaveAsync(
        WorkflowRunResponse workflowRun,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflowRun);
        cancellationToken.ThrowIfCancellationRequested();

        _runs[workflowRun.Id] = workflowRun;

        return Task.CompletedTask;
    }

    public Task<WorkflowRunResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _runs.TryGetValue(id, out var workflowRun);

        return Task.FromResult(workflowRun);
    }
}
