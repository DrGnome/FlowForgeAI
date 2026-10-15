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

    public async Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        var result = await _aiService.AnalyzeAsync(
            goal,
            cancellationToken);

        return System.Text.Json.JsonSerializer.Serialize(result);
    }
}