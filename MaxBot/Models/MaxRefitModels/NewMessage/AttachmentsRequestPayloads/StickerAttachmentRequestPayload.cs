using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

/// <summary>
/// RU: Запрос на прикрепление стикера
/// ENG: Sticker attachment request payload
/// </summary>
public record StickerAttachmentRequestPayload
{
    /// <summary>
    /// RU: Код стикера
    /// ENG: Sticker code
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}