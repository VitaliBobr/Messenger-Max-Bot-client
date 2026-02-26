using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Update;

/// <summary>
/// RU: Модель callback-запроса от inline-кнопки
/// ENG: Callback query model from inline button
/// </summary>
public record Callback
{
    /// <summary>
    /// RU: Unix-время, когда пользователь нажал кнопку
    /// ENG: Unix timestamp when the user pressed the button
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// RU: Текущий ID клавиатуры
    /// ENG: Current keyboard ID
    /// </summary>
    [JsonPropertyName("callback_id")]
    public string CallbackId { get; set; } = string.Empty;

    /// <summary>
    /// RU: Токен кнопки (опционально)
    /// ENG: Button token (optional)
    /// </summary>
    [JsonPropertyName("payload")]
    public string? Payload { get; set; }

    /// <summary>
    /// RU: Пользователь, нажавший на кнопку
    /// ENG: User who pressed the button
    /// </summary>
    [JsonPropertyName("user")]
    public User User { get; set; } = null!;
}