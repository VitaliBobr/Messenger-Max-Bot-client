using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// RU: Модель сообщения в чате
/// ENG: Message model in chat
/// </summary>
public record Message
{
    /// <summary>
    /// RU: Пользователь, отправивший сообщение. Может отсутствовать для служебных сообщений.
    /// ENG: User who sent the message. May be absent for service messages.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// RU: Получатель сообщения (пользователь или чат)
    /// ENG: Message recipient (user or chat)
    /// </summary>
    [JsonPropertyName("recipient")]
    public Recipient Recipient { get; set; } = new();

    /// <summary>
    /// RU: Время создания сообщения в формате Unix-time (миллисекунды)
    /// ENG: Message creation time in Unix-time format (milliseconds)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// RU: Пересланное или ответное сообщение
    /// ENG: Forwarded or replied message
    /// </summary>
    [JsonPropertyName("link")]
    public LinkedMessage? Link { get; set; }

    /// <summary>
    /// RU: Содержимое сообщения (текст + вложения). 
    /// Может быть null, если сообщение содержит только пересланное сообщение.
    /// ENG: Message content (text + attachments).
    /// May be null if message only contains a forwarded message.
    /// </summary>
    [JsonPropertyName("body")]
    public MessageBody? Body { get; set; }

    /// <summary>
    /// RU: Статистика сообщения (просмотры, лайки)
    /// ENG: Message statistics (views, likes)
    /// </summary>
    [JsonPropertyName("stat")]
    public MessageStat? Stat { get; set; }

    /// <summary>
    /// RU: Публичная ссылка на пост в канале. Отсутствует для диалогов и групповых чатов.
    /// ENG: Public link to the channel post. Absent for dialogs and group chats.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}