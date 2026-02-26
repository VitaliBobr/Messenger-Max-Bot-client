using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос списка администраторов группового чата.
/// </summary>
public record GetChatAdminsResponse
{
    /// <summary>
    /// Список администраторов чата (объекты ChatMember).
    /// </summary>
    [JsonPropertyName("members")]
    public List<ChatMember> Members { get; set; } = new();

    /// <summary>
    /// Маркер для получения следующей страницы списка (null, если страница последняя).
    /// </summary>
    [JsonPropertyName("marker")]
    public long? Marker { get; set; }
}