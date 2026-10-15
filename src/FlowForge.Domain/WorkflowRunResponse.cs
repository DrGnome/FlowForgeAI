namespace FlowForge.Domain;

public sealed record WorkflowRunResponse(
    Guid Id,
    string Goal,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    AnalysisResult? Analysis,
    IReadOnlyList<WorkflowStepResult> Steps);