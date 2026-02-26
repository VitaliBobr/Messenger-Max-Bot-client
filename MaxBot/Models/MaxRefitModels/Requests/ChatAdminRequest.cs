using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Requests;

/// <summary>
/// Модель администратора для назначения (используется в теле запроса POST /chats/{chatId}/members/admins).
/// </summary>
public record ChatAdminRequest
{
    /// <summary>
    /// ID пользователя, который становится администратором.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    /// Список прав, которые получает администратор.
    /// Возможные значения: "read_all_messages", "add_remove_members", "add_admins",
    /// "change_chat_info", "pin_message", "write".
    /// </summary>
    [JsonPropertyName("permissions")]
    public List<ChatAdminPermission> Permissions { get; set; } = new();

    /// <summary>
    /// Отображаемый заголовок администратора (опционально).
    /// </summary>
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
}

/// <summary>
/// Запрос на назначение администраторов в групповом чате.
/// </summary>
public record AddAdminsRequest
{
    /// <summary>
    /// Список пользователей, которые получат права администратора.
    /// </summary>
    [JsonPropertyName("admins")]
    public List<ChatAdminRequest> Admins { get; set; } = new();
}