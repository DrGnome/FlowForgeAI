
using FlowForge.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WorkflowRunner>();

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
