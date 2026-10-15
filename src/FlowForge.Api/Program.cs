using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FlowForge.Infrastructure;
using FlowForge.Application;
using FlowForge.Application.Workflows;
using FlowForge.Application.AI;
using FlowForge.Infrastructure.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<AiOptions>(
        builder.Configuration.GetSection("Ai"));

builder.Services.AddHttpClient<IAiService, OllamaAiService>();

builder.Services.AddTransient<IWorkflowStep, ValidateGoalStep>();
builder.Services.AddTransient<IWorkflowStep, PrepareAnalysisStep>();
builder.Services.AddTransient<IWorkflowStep, GenerateResultStep>();

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

app.Run();

public sealed record RunWorkflowRequest(
    string Goal,
    string DocumentText);

public partial class Program { }