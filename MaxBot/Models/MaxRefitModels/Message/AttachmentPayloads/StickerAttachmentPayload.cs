using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

/// <summary>
/// RU: Данные стикера
/// ENG: Sticker attachment payload
/// </summary>
public record StickerAttachmentPayload
{
    /// <summary>
    /// RU: URL стикера
    /// ENG: Sticker URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// RU: ID стикера
    /// ENG: Sticker code/ID
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}