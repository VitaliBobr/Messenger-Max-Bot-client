using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Chat;

/// <summary>
/// RU: Модель иконки чата
/// ENG: Chat icon model
/// </summary>
public record Image
{
    /// <summary>
    /// RU: URL изображения
    /// ENG: Image URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}