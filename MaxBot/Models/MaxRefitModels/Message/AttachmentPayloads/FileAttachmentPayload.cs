using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

/// <summary>
/// RU: Базовые данные файла-вложения
/// ENG: Base file attachment payload
/// </summary>
public abstract record FileAttachmentPayload
{
    /// <summary>
    /// RU: URL файл-вложения. Будет получен в Update после отправки
    /// ENG: File attachment URL. Will be received in Update after sending
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// RU: Токен для повторного использования вложения
    /// ENG: Token for reusing the same attachment in another message
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}