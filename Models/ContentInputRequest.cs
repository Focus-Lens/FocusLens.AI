namespace FocusLens.AI.Models;

public class ContentInputRequest
{
    public string InputType { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}