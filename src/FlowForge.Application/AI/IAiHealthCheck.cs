namespace FlowForge.Application.AI;

public interface IAiHealthCheck
{
    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default);
}