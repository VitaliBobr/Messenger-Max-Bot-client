using System.Net.Mail;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// RU: Содержимое сообщения
/// ENG: Message content
/// </summary>
public record MessageBody
{
    /// <summary>
    /// RU: Уникальный ID сообщения
    /// ENG: Unique message ID
    /// </summary>
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    /// <summary>
    /// RU: ID последовательности сообщения в чате
    /// ENG: Message sequence ID in chat
    /// </summary>
    [JsonPropertyName("seq")]
    public long Seq { get; set; }

    /// <summary>
    /// RU: Текст сообщения. Может быть null, если только пересланное сообщение
    /// ENG: Message text. May be null if only forwarded content
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// RU: Вложения сообщения
    /// ENG: Message attachments
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<Attachment>? Attachments { get; set; }

    /// <summary>
    /// RU: Разметка текста (жирный, курсив, ссылки и т.д.)
    /// ENG: Text markup (bold, italic, links, etc.)
    /// </summary>
    [JsonPropertyName("markup")]
    public List<MarkupElement>? Markup { get; set; }
}