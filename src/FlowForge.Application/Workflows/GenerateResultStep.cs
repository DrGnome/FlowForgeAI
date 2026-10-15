
using FlowForge.Application.AI;

namespace FlowForge.Application.Workflows;

public sealed class GenerateResultStep : IWorkflowStep
{
    private readonly IAiService _aiService;

    public GenerateResultStep(IAiService aiService)
    {
        _aiService = aiService;
    }

    public string Name => "Generate result";

    public Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        return _aiService.AnalyzeAsync(
            goal,
            cancellationToken);
    }
}
