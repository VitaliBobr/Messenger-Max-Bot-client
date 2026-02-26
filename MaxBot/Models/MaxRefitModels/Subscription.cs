using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels;

public record Subscription
{
    /// <summary>
    /// URL вебхука для получения обновлений.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Время создания подписки (Unix timestamp в миллисекундах).
    /// </summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    /// <summary>
    /// Типы обновлений, на которые подписан бот.
    /// (Значение может быть строкой или списком – уточните по документации.)
    /// </summary>
    [JsonPropertyName("update_types")]
    public string UpdateTypes { get; set; } = string.Empty;
}