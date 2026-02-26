using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

namespace MaxBot.Models;

/// <summary>
/// Запрос на изменение информации о групповом чате.
/// Все поля опциональны — отправляются только те, которые нужно изменить.
/// </summary>
public record UpdateChatRequest
{
    /// <summary>
    /// Новая иконка чата. Должна быть одним из вариантов ImageRequestPayload:
    /// - FromUrl: { "url": "..." }
    /// - FromToken: { "token": "..." }
    /// - FromPhotos: { "photos": { "token": "..." } }
    /// </summary>
    [JsonPropertyName("icon")]
    public ImageRequestPayload? Icon { get; set; }

    /// <summary>
    /// Новое название чата (от 1 до 200 символов).
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// ID сообщения для закрепления.
    /// </summary>
    [JsonPropertyName("pin")]
    public string? Pin { get; set; }

    /// <summary>
    /// Отправлять ли уведомление участникам об изменении (по умолчанию true).
    /// </summary>
    [JsonPropertyName("notify")]
    public bool? Notify { get; set; }
}