using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос получения сообщений.
/// </summary>
public record GetMessagesResponse
{
    /// <summary>
    /// Массив сообщений.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<Message.Message> Messages { get; set; } = new();
}