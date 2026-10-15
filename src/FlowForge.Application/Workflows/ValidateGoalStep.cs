
namespace FlowForge.Application.Workflows;

public sealed class ValidateGoalStep : IWorkflowStep
{
    public string Name => "Validate goal";

    public Task<string> ExecuteAsync(
        string goal,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(goal))
        {
            throw new ArgumentException(
                "A workflow goal is required.",
                nameof(goal));
        }

        return Task.FromResult("The workflow goal is valid.");
    }
}
