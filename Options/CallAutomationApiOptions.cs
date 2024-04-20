using System.ComponentModel.DataAnnotations;

namespace Options;


public sealed class CallAutomationApiOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}