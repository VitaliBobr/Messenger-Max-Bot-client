using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// Preview video
/// </summary>
public record VideoThumbnail
{
    /// <summary>
    /// Url to image
    /// </summary>
    [JsonPropertyName("url")]
    string Url  { get; init; } = String.Empty;
}