using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Message;

namespace MaxBot.Models.MaxRefitModels.NewMessage;

/// <summary>
/// RU: Ссылка на другое сообщение (для ответов или пересылки)
/// Используется при отправке сообщений
/// ENG: Link to another message (for replies or forwarding)
/// Used when sending messages
/// </summary>
public record NewMessageLink
{
    /// <summary>
    /// RU: Тип ссылки
    /// ENG: Link type
    /// </summary>
    [JsonPropertyName("type")]
    public MessageLinkType Type { get; set; }

    /// <summary>
    /// RU: ID сообщения, на которое ссылаемся
    /// ENG: ID of the referenced message
    /// </summary>
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;
}