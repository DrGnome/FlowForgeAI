using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FlowForge.Infrastructure;
using FlowForge.Application;
using FlowForge.Application.Workflows;
using FlowForge.Application.AI;
using FlowForge.Infrastructure.AI;
using FlowForge.Application.WorkflowHistory;
using FlowForge.Infrastructure.WorkflowHistory;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AiOptions>()
    .Bind(builder.Configuration.GetSection(AiOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp
                || uri.Scheme == Uri.UriSchemeHttps),
        "Ai:BaseUrl must be an absolute HTTP or HTTPS URL.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IAiService, OllamaAiService>(
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(90);
    });
builder.Services.AddHttpClient<IAiHealthCheck, OllamaHealthCheck>();

builder.Services.AddTransient<IWorkflowStep, ValidateGoalStep>();
builder.Services.AddTransient<IWorkflowStep, PrepareAnalysisStep>();
builder.Services.AddTransient<IWorkflowStep, GenerateResultStep>();
builder.Services.AddSingleton<
    IWorkflowRunRepository,
    InMemoryWorkflowRunRepository>();

builder.Services.AddTransient<WorkflowRunner>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    name = "FlowForge AI",
    status = "Running"
}));

app.MapPost("/workflows/run", async (
    RunWorkflowRequest request,
    WorkflowRunner runner,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Goal) ||
        request.Goal.Length > 4000)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["goal"] =
                [
                    "The goal must contain between 1 and 4000 characters."
                ]
            });
    }

    if (string.IsNullOrWhiteSpace(request.DocumentText) ||
        request.DocumentText.Length > 20000)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["documentText"] =
                [
                    "Document text is required and must contain no more than 20000 characters."
                ]
            });
    }

    var input = new WorkflowInput(
        request.Goal,
        request.DocumentText);

    var result = await runner.RunAsync(
        input,
        cancellationToken);

    if (result.Status == "Failed")
    {
        return Results.Problem(
            title: "Workflow execution failed",
            detail: "The workflow could not be completed.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Ok(result);
});

app.MapGet("/health", async (
    IAiHealthCheck aiHealthCheck,
    CancellationToken cancellationToken) =>
{
    var aiAvailable = await aiHealthCheck.IsAvailableAsync(
        cancellationToken);

    var response = new
    {
        status = aiAvailable ? "Healthy" : "Degraded",
        api = "Healthy",
        aiProvider = aiAvailable ? "Available" : "Unavailable",
        timestamp = DateTimeOffset.UtcNow
    };

    return aiAvailable
        ? Results.Ok(response)
        : Results.Json(response, statusCode: 503);
});


app.MapGet("/workflows/{id:guid}", async (
    Guid id,
    IWorkflowRunRepository repository,
    CancellationToken cancellationToken) =>
{
    var workflowRun = await repository.GetByIdAsync(
        id,
        cancellationToken);

    return workflowRun is null
        ? Results.NotFound(new { error = "Workflow run not found." })
        : Results.Ok(workflowRun);
});


app.Run();

public sealed record RunWorkflowRequest(
    string Goal,
    string DocumentText);

public partial class Program { }