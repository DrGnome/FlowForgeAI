using FlowForge.Domain;

namespace FlowForge.Application.Workflows;

public sealed record WorkflowStepOutput(
    string Output,
    AnalysisResult? Analysis = null);