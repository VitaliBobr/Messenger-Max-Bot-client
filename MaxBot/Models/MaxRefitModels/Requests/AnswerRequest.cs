using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.NewMessage;

namespace MaxBot.Models.MaxRefitModels.Answers;

/// <summary>
/// Запрос на ответ на callback после нажатия кнопки.
/// </summary>
public record AnswerRequest
{
    /// <summary>
    /// Новое сообщение для замены текущего (опционально).
    /// </summary>
    [JsonPropertyName("message")]
    public NewMessageBody? Message { get; set; }

    /// <summary>
    /// Текст одноразового уведомления для пользователя (опционально).
    /// </summary>
    [JsonPropertyName("notification")]
    public string? Notification { get; set; }
}