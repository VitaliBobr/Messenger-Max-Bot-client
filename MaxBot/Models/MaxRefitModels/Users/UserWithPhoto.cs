using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Users;

/// <summary>
/// RU: Информация о пользователе или боте, включающая аватар и описание
/// ENG: Information about a user or bot, including avatar and description
/// </summary>
public record UserWithPhoto : User
{
    /// <summary>
    /// RU: Описание пользователя или бота.
    /// Может быть null, если не заполнено. Максимальная длина: 16000 символов.
    /// ENG: Description of user or bot.
    /// May be null if not set. Maximum length: 16000 characters.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// RU: URL аватара пользователя или бота в уменьшенном размере
    /// ENG: URL of user's or bot's avatar in reduced size
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// RU: URL аватара пользователя или бота в полном размере
    /// ENG: URL of user's or bot's avatar in full size
    /// </summary>
    [JsonPropertyName("full_avatar_url")]
    public string? FullAvatarUrl { get; set; }
}