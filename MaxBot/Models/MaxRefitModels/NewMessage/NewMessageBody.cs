using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage;

/// <summary>
/// RU: Тело нового сообщения для отправки или редактирования.
/// Используется в запросах к API для создания или изменения сообщений.
/// ENG: New message body for sending or editing.
/// Used in API requests to create or modify messages.
/// </summary>
public record NewMessageBody
{
    /// <summary>
    /// RU: Текст сообщения. Максимальная длина: 4000 символов.
    /// Может быть null, если сообщение содержит только вложения.
    /// ENG: Message text. Maximum length: 4000 characters.
    /// May be null if message has only attachments.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// RU: Вложения сообщения. Если null или пустой список, все вложения будут удалены (при редактировании).
    /// ENG: Message attachments. If null or empty, all attachments will be removed (when editing).
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<AttachmentRequest>? Attachments { get; set; }

    /// <summary>
    /// RU: Ссылка на другое сообщение (для ответа или пересылки)
    /// ENG: Link to another message (for reply or forward)
    /// </summary>
    [JsonPropertyName("link")]
    public NewMessageLink? Link { get; set; }

    /// <summary>
    /// RU: Отправлять ли уведомление участникам чата.
    /// По умолчанию: true.
    /// ENG: Whether to notify chat participants.
    /// Default: true.
    /// </summary>
    [JsonPropertyName("notify")]
    public bool? Notify { get; set; }

    /// <summary>
    /// RU: Формат текста сообщения (markdown или html).
    /// Если установлен, текст будет форматирован соответствующим образом.
    /// ENG: Message text format (markdown or html).
    /// If set, the text will be formatted accordingly.
    /// </summary>
    [JsonPropertyName("format")]
    public TextFormat? Format { get; set; }
}