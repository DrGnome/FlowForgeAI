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

    public async Task<WorkflowStepOutput> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        var analysis = await _aiService.AnalyzeAsync(
            input.Goal,
            input.DocumentText,
            cancellationToken);

        var output =
            $"Summary: {analysis.Summary}{Environment.NewLine}" +
            $"Key findings: {analysis.KeyFindings.Count}{Environment.NewLine}" +
            $"Caveats: {analysis.Caveats.Count}";

        return new WorkflowStepOutput(output, analysis);
    }
}