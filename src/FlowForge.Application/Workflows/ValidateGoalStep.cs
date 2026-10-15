
namespace FlowForge.Application.Workflows;

public sealed class ValidateGoalStep : IWorkflowStep
{
    public string Name => "Validate goal";

    public Task<string> ExecuteAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(input.Goal))
        {
            throw new ArgumentException(
                "A workflow goal is required.",
                nameof(input));
        }

        return Task.FromResult("The workflow goal is valid.");
    }
}
