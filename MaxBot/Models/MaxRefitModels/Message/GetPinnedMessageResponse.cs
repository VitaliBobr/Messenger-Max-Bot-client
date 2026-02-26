using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// Ответ на запрос получения закреплённого сообщения в групповом чате.
/// </summary>
public record GetPinnedMessageResponse
{
    /// <summary>
    /// Закреплённое сообщение. Может быть null, если в чате нет закреплённого сообщения.
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }
}