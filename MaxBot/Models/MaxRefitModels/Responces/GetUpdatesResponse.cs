using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос получения обновлений через long polling.
/// </summary>
public record GetUpdatesResponse
{
    /// <summary>
    /// Список обновлений (событий), произошедших в чатах.
    /// </summary>
    [JsonPropertyName("updates")]
    public List<Update.Update> Updates { get; set; } = new();

    /// <summary>
    /// Указатель на следующую страницу обновлений.
    /// Передайте это значение в параметре marker следующего запроса.
    /// </summary>
    [JsonPropertyName("marker")]
    public long? Marker { get; set; }
}