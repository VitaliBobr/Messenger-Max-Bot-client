using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// RU: Тип чата
/// ENG: Chat type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatType
{
    [EnumMember(Value = "chat")]
    Chat  // групповой чат
    
    // По документации других значений пока нет
}

/// <summary>
/// RU: Получатель сообщения (пользователь или чат)
/// ENG: Message recipient (user or chat)
/// </summary>
public record Recipient
{
    /// <summary>
    /// RU: ID чата (если сообщение в чат)
    /// ENG: Chat ID (if message to chat)
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// RU: Тип чата
    /// ENG: Chat type
    /// </summary>
    [JsonPropertyName("chat_type")]
    public ChatType? ChatType { get; set; }

    /// <summary>
    /// RU: ID пользователя (если личное сообщение)
    /// ENG: User ID (if private message)
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
}