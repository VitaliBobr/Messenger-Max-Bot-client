using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Requests;

/// <summary>
/// Запрос на добавление участников в групповой чат.
/// </summary>
public record AddMembersRequest
{
    /// <summary>
    /// Массив идентификаторов пользователей для добавления в чат.
    /// </summary>
    [JsonPropertyName("user_ids")]
    public List<long> UserIds { get; set; } = new();
}