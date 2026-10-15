using System.ComponentModel.DataAnnotations;

namespace FlowForge.Infrastructure.AI;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    [Required]
    [Url]
    public string BaseUrl { get; set; } =
        "http://localhost:11434/";

    [Required]
    [MinLength(1)]
    public string Model { get; set; } = "llama3.2";
}