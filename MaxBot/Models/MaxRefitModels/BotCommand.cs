using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels;

/// <summary>
/// RU: Модель команды бота
/// ENG: Bot command model
/// </summary>
public record BotCommand: UserWithPhoto
{
    /// <summary>
    /// RU: Название команды (например, /start)
    /// ENG: Command name (e.g., /start)
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// RU: Описание команды
    /// ENG: Command description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; } = string.Empty;
}