using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос добавления участников в групповой чат.
/// </summary>
public record AddMembersResponse
{
    /// <summary>
    /// true, если все пользователи успешно добавлены, иначе false.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Объяснительное сообщение, если результат не был успешным.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}