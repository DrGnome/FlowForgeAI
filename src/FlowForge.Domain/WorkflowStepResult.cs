namespace FlowForge.Domain;

public sealed record WorkflowStepResult(
    string Name,
    string Status,
    string Output,
    DateTimeOffset CompletedAt);