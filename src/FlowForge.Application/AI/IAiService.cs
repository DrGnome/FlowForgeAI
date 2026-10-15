using FlowForge.Domain;

namespace FlowForge.Application.AI;

public interface IAiService
{
    Task<AnalysisResult> AnalyzeAsync(
        string goal,
        string documentText,
        CancellationToken cancellationToken = default);
}