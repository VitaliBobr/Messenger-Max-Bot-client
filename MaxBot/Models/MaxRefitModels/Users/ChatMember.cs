using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Users;

/// <summary>
/// RU: Информация об участнике чата.
/// Возвращается методами группы /chats, например GET /chats/{chatId}/members.
/// ENG: Chat member information.
/// Returned by /chats group methods, e.g., GET /chats/{chatId}/members.
/// </summary>
public record ChatMember : UserWithPhoto
{
    /// <summary>
    /// RU: Время последней активности пользователя в чате (Unix ms).
    /// Может быть устаревшим для суперчатов (равно времени вступления).
    /// ENG: User's last activity time in chat (Unix ms).
    /// May be outdated for superchats (equals join time).
    /// </summary>
    [JsonPropertyName("last_access_time")]
    public long LastAccessTime { get; set; }

    /// <summary>
    /// RU: Является ли пользователь владельцем чата
    /// ENG: Is the user the chat owner
    /// </summary>
    [JsonPropertyName("is_owner")]
    public bool IsOwner { get; set; }

    /// <summary>
    /// RU: Является ли пользователь администратором чата
    /// ENG: Is the user a chat administrator
    /// </summary>
    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }

    /// <summary>
    /// RU: Дата присоединения к чату (Unix ms)
    /// ENG: Date when user joined the chat (Unix ms)
    /// </summary>
    [JsonPropertyName("join_time")]
    public long JoinTime { get; set; }

    /// <summary>
    /// RU: Перечень прав пользователя в чате
    /// ENG: List of user permissions in chat
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<ChatAdminPermission>? Permissions { get; set; }

    /// <summary>
    /// RU: Заголовок, который будет показан на клиенте.
    /// Если пользователь администратор или владелец и ему не установлено это название,
    /// поле не передаётся, клиенты подменяют на "владелец" или "админ".
    /// ENG: Custom title displayed on the client.
    /// If not set for admin/owner, clients will show "owner" or "admin" instead.
    /// </summary>
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
}