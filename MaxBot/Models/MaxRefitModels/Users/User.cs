using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Users;

/// <summary>
/// RU: Модель пользователя или бота в мессенджере MAX
/// ENG: User or bot model in MAX messenger
/// </summary>
public record User
{
    /// <summary>
    /// RU: Идентификатор пользователя или бота
    /// ENG: User or bot unique identifier
    /// </summary>
    [JsonPropertyName("user_id")] 
    public long UserId { get; set; }
    
    /// <summary>
    /// RU: Отображаемое имя пользователя или бота
    /// ENG: Display name of user or bot
    /// </summary>
    [JsonPropertyName("first_name")] 
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// RU: Фамилия пользователя. Для бота будет null
    /// ENG: User last name. Will be null for bots
    /// </summary>
    [JsonPropertyName("last_name")] 
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>  
    /// RU: Уникальное публичное имя пользователя или бота.
    /// Для пользователя может быть null, если имя не задано или пользователь недоступен
    /// ENG: Unique public username of user or bot.
    /// May be null for users if not set or user is unavailable
    /// </summary>
    [JsonPropertyName("username")] 
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// RU: Является ли учётная запись ботом
    /// ENG: Indicates if this account is a bot
    /// </summary>
    [JsonPropertyName("is_bot")] 
    public bool IsBot { get; set; } = false;
    
    /// <summary>
    /// RU: Время последней активности пользователя или бота в MAX
    /// (Unix-время в миллисекундах).
    /// Поле может отсутствовать, если пользователь отключил отображение онлайн-статуса
    /// ENG: Last activity time of user or bot in MAX
    /// (Unix timestamp in milliseconds).
    /// May be omitted if user disabled online status visibility
    /// </summary>
    [JsonPropertyName("last_activity_time")] 
    public long? LastActivityTime { get; set; }
}