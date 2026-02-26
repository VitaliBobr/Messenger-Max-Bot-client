using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

/// <summary>
/// RU: Запрос на прикрепление inline-клавиатуры
/// ENG: Inline keyboard attachment request payload
/// </summary>
public record InlineKeyboardAttachmentRequestPayload
{
    /// <summary>
    /// RU: Двумерный массив кнопок (ряды и колонки)
    /// ENG: Two-dimensional array of buttons (rows and columns)
    /// </summary>
    [JsonPropertyName("buttons")]
    public List<List<Button>> Buttons { get; set; } = new();
}