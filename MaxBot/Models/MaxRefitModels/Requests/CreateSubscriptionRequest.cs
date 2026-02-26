using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Requests;

/// <summary>
/// Запрос на создание подписки для получения обновлений через Webhook.
/// </summary>
public record CreateSubscriptionRequest
{
    /// <summary>
    /// URL HTTP(S)-эндпойнта вашего бота. Должен начинаться с http(s)://
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Список типов обновлений, которые ваш бот хочет получать.
    /// </summary>
    [JsonPropertyName("update_types")]
    public List<string>? UpdateTypes { get; set; }

    /// <summary>
    /// Секрет для заголовка X-Max-Bot-Api-Secret. Разрешены символы A-Z, a-z, 0-9, дефис.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }
}