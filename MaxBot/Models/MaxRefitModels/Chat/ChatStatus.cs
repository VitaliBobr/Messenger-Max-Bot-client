using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Chat;

/// <summary>
/// RU: Статус чата для бота
/// ENG: Chat status for the bot
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatStatus
{
    /// <summary>
    /// RU: Бот является активным участником чата
    /// ENG: Bot is an active member of the chat
    /// </summary>
    [EnumMember(Value = "active")]
    Active,

    /// <summary>
    /// RU: Бот был удалён из чата
    /// ENG: Bot was removed from the chat
    /// </summary>
    [EnumMember(Value = "removed")]
    Removed,

    /// <summary>
    /// RU: Бот покинул чат
    /// ENG: Bot left the chat
    /// </summary>
    [EnumMember(Value = "left")]
    Left,

    /// <summary>
    /// RU: Чат был закрыт
    /// ENG: Chat was closed
    /// </summary>
    [EnumMember(Value = "closed")]
    Closed
}