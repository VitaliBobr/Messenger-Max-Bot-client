using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Users;

/// <summary>
/// RU: Информация о боте с поддерживаемыми командами.
/// Возвращается только методом GET /me.
/// ENG: Bot information with supported commands.
/// Returned only by GET /me method.
/// </summary>
public record BotInfo : UserWithPhoto
{
    /// <summary>
    /// RU: Список команд, поддерживаемых ботом. Максимум 32 элемента.
    /// ENG: List of commands supported by the bot. Maximum 32 items.
    /// </summary>
    [JsonPropertyName("commands")]
    public List<BotCommand>? Commands { get; set; }
};