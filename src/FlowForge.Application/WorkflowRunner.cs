using System.Diagnostics;
using FlowForge.Application.Workflows;
using FlowForge.Domain;
using Microsoft.Extensions.Logging;
using FlowForge.Application.WorkflowHistory;

namespace FlowForge.Application;

public sealed class WorkflowRunner
{
    private readonly IReadOnlyList<IWorkflowStep> _steps;
    private readonly ILogger<WorkflowRunner> _logger;
    private readonly IWorkflowRunRepository _workflowRunRepository;

    public WorkflowRunner(
        IEnumerable<IWorkflowStep> steps,
        ILogger<WorkflowRunner> logger,
        IWorkflowRunRepository workflowRunRepository)
    {
        _steps = steps.ToArray();
        _logger = logger;
        _workflowRunRepository = workflowRunRepository;
    }

    public async Task<WorkflowRunResponse> RunAsync(
        WorkflowInput input,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var results = new List<WorkflowStepResult>();
        var status = "Succeeded";
        AnalysisResult? analysis = null;

        _logger.LogInformation(
            "Workflow {WorkflowId} started. Goal length: {GoalLength}.",
            id,
            input.Goal.Length);

        foreach (var step in _steps)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                status = "Cancelled";

                _logger.LogWarning(
                    "Workflow {WorkflowId} cancelled before step {StepName}.",
                    id,
                    step.Name);

                break;
            }

            var stepStartedAt = Stopwatch.StartNew();

            _logger.LogInformation(
                "Workflow {WorkflowId}: step {StepName} started.",
                id,
                step.Name);

            try
            {
                var stepOutput = await step.ExecuteAsync(
                    input,
                    cancellationToken);

                if (stepOutput.Analysis is not null)
                {
                    analysis = stepOutput.Analysis;
                }

                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Succeeded",
                    stepOutput.Output,
                    DateTimeOffset.UtcNow));

                _logger.LogInformation(
                    "Workflow {WorkflowId}: step {StepName} succeeded in {ElapsedMilliseconds} ms.",
                    id,
                    step.Name,
                    stepStartedAt.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Cancelled",
                    "Step execution was cancelled.",
                    DateTimeOffset.UtcNow));

                status = "Cancelled";

                _logger.LogWarning(
                    "Workflow {WorkflowId}: step {StepName} was cancelled.",
                    id,
                    step.Name);

                break;
            }
            catch (Exception exception)
            {
                results.Add(new WorkflowStepResult(
                    step.Name,
                    "Failed",
                    "Step execution failed. See application logs for details.",
                    DateTimeOffset.UtcNow));

                status = "Failed";

                _logger.LogError(
                    exception,
                    "Workflow {WorkflowId}: step {StepName} failed after {ElapsedMilliseconds} ms.",
                    id,
                    step.Name,
                    stepStartedAt.ElapsedMilliseconds);

                break;
            }
        }

        stopwatch.Stop();

        var response = new WorkflowRunResponse(
            id,
            input.Goal,
            status,
            startedAt,
            DateTimeOffset.UtcNow,
            analysis,
            results);

        var persistenceToken = status == "Cancelled"
            ? CancellationToken.None
            : cancellationToken;

        await _workflowRunRepository.SaveAsync(response, persistenceToken);
        if (status == "Succeeded")
        {
            _logger.LogInformation(
                "Workflow {WorkflowId} completed successfully in {ElapsedMilliseconds} ms.",
                id,
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogWarning(
                "Workflow {WorkflowId} finished with status {WorkflowStatus} in {ElapsedMilliseconds} ms.",
                id,
                status,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}