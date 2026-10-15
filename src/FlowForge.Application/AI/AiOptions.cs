
namespace FlowForge.Infrastructure.AI;

public sealed class AiOptions
{
    public string BaseUrl { get; set; } =
        "http://localhost:11434/v1/";

    public string Model { get; set; } = "llama3.2";
}
