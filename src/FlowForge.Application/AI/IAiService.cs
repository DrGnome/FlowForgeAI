
namespace FlowForge.Application.AI;

public interface IAiService
{
    Task<string> AnalyzeAsync(
        string goal,
        CancellationToken cancellationToken = default);
}
