using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// RU: Тип связанного сообщения
/// ENG: Linked message type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageLinkType
{
    [EnumMember(Value = "forward")]
    Forward,
    
    [EnumMember(Value = "reply")]
    Reply
}

/// <summary>
/// RU: Пересланное или ответное сообщение
/// ENG: Forwarded or replied message
/// </summary>
public record LinkedMessage
{
    /// <summary>
    /// RU: Тип связанного сообщения
    /// ENG: Linked message type
    /// </summary>
    [JsonPropertyName("type")]
    public MessageLinkType Type { get; set; }

    /// <summary>
    /// RU: Пользователь, отправивший исходное сообщение
    /// ENG: User who sent the original message
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// RU: Чат, в котором сообщение было изначально опубликовано
    /// Только для пересланных сообщений
    /// ENG: Chat where the message was originally published
    /// Only for forwarded messages
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// RU: Тело сообщения
    /// ENG: Message body
    /// </summary>
    [JsonPropertyName("message")]
    public MessageBody? Message { get; set; }
}