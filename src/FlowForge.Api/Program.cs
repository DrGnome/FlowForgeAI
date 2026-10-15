
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

app.MapPost(
    "/workflows/run",
    async (
        RunWorkflowRequest request,
        WorkflowRunner runner,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(request.Goal))
        {
            return Results.BadRequest(new
            {
                error = "A workflow goal is required."
            });
        }

        var result = await runner.RunAsync(
            request.Goal,
            cancellationToken);

        return Results.Ok(result);
    });

app.Run();

public sealed record RunWorkflowRequest(string Goal);
