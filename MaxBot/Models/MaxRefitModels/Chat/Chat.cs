using System.Net.Mime;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Chat;

public record Chat
{
    /// <summary>
    /// RU: Уникальный идентификатор чата
    /// ENG: Unique chat identifier
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    /// <summary>
    /// RU: Тип чата
    /// ENG: Chat type
    /// </summary>
    [JsonPropertyName("type")]
    public ChatType Type { get; set; }

    /// <summary>
    /// RU: Статус чата для бота
    /// ENG: Chat status for the bot
    /// </summary>
    [JsonPropertyName("status")]
    public ChatStatus Status { get; set; }

    /// <summary>
    /// RU: Отображаемое название чата. Может быть null для диалогов.
    /// ENG: Chat title. May be null for dialogs.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// RU: Иконка чата
    /// ENG: Chat icon
    /// </summary>
    [JsonPropertyName("icon")]
    public Image? Icon { get; set; }

    /// <summary>
    /// RU: Время последнего события в чате (Unix ms)
    /// ENG: Time of the last event in the chat (Unix ms)
    /// </summary>
    [JsonPropertyName("last_event_time")]
    public long LastEventTime { get; set; }

    /// <summary>
    /// RU: Количество участников чата. Для диалогов всегда 2.
    /// ENG: Number of participants. Always 2 for dialogs.
    /// </summary>
    [JsonPropertyName("participants_count")]
    public int ParticipantsCount { get; set; }

    /// <summary>
    /// RU: ID владельца чата
    /// ENG: ID of the chat owner
    /// </summary>
    [JsonPropertyName("owner_id")]
    public long? OwnerId { get; set; }

    /// <summary>
    /// RU: Участники чата с временем последней активности.
    /// Может быть null, если запрашивается список чатов.
    /// ENG: Chat participants with last activity time.
    /// May be null when requesting a list of chats.
    /// TODO Поле вообще хз будь на чеку
    /// </summary>
    [JsonPropertyName("participants")]
    public ParticipantsInfo? Participants { get; set; }

    /// <summary>
    /// RU: Доступен ли чат публично (для диалогов всегда false)
    /// ENG: Is the chat publicly available (always false for dialogs)
    /// </summary>
    [JsonPropertyName("is_public")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// RU: Ссылка на чат
    /// ENG: Chat invite link
    /// </summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>
    /// RU: Описание чата
    /// ENG: Chat description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// RU: Данные о пользователе в диалоге (только для чатов типа "dialog").
    /// ENG: User data for a dialog (only for chats of type "dialog").
    /// </summary>
    [JsonPropertyName("dialog_with_user")]
    public UserWithPhoto? DialogWithUser { get; set; }

    /// <summary>
    /// RU: ID сообщения, содержащего кнопку, через которую был инициирован чат
    /// ENG: ID of the message containing the button that initiated the chat
    /// </summary>
    [JsonPropertyName("chat_message_id")]
    public string? ChatMessageId { get; set; }

    /// <summary>
    /// RU: Закреплённое сообщение в чате (возвращается только при запросе конкретного чата)
    /// ENG: Pinned message in the chat (returned only when requesting a specific chat)
    /// </summary>
    [JsonPropertyName("pinned_message")]
    public Message.Message? PinnedMessage { get; set; }
}