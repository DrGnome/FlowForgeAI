namespace FlowForge.Domain;

public sealed record AnalysisResult(
    string Summary,
    IReadOnlyList<string> KeyFindings,
    IReadOnlyList<string> Caveats);