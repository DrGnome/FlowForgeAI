namespace FlowForge.Domain;

public sealed record WorkflowRun(
    Guid Id,
    string Goal,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    IReadOnlyList<WorkflowStepResult> Steps);