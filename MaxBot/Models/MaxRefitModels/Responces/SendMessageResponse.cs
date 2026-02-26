using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос отправки сообщения.
/// </summary>
public record SendMessageResponse
{
    /// <summary>
    /// Отправленное сообщение.
    /// </summary>
    [JsonPropertyName("message")]
    public Message.Message Message { get; set; } = null!;
}