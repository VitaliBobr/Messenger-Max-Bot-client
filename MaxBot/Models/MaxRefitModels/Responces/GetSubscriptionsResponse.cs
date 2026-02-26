using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Ответ на запрос получения списка подписок.
/// </summary>
public record GetSubscriptionsResponse
{
    /// <summary>
    /// Список текущих подписок.
    /// </summary>
    [JsonPropertyName("subscriptions")]
    public List<Subscription> Subscriptions { get; set; } = new();
}