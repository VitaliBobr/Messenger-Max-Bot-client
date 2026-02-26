using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

/// <summary>
/// RU: Данные вложения-изображения
/// ENG: Image attachment payload
/// </summary>
public record PhotoAttachmentPayload
{
    /// <summary>
    /// RU: Уникальный ID изображения
    /// ENG: Unique image ID
    /// </summary>
    [JsonPropertyName("photo_id")]
    public long PhotoId { get; set; }

    /// <summary>
    /// RU: Токен доступа к изображению
    /// ENG: Image access token
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// RU: URL изображения
    /// ENG: Image URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}