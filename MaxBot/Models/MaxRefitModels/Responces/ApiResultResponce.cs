using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос удаления группового чата.
/// </summary>
public record ApiResultResponce
{
    /// <summary>
    /// true, если чат успешно удалён, иначе false.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Опциональное пояснение, если success = false.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}