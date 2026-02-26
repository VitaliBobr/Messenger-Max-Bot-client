using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

/// <summary>
/// RU: Данные расшариваемой ссылки
/// ENG: Share attachment payload
/// </summary>
public record ShareAttachmentPayload
{
    /// <summary>
    /// RU: URL для предпросмотра (опционально)
    /// ENG: URL for preview (optional)
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// RU: Токен вложения (опционально)
    /// ENG: Attachment token (optional)
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}