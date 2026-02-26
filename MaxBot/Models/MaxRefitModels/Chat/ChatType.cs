using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Chat;

/// <summary>
/// RU: Тип чата
/// ENG: Chat type
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatType
{
    /// <summary>
    /// RU: Групповой чат
    /// ENG: Group chat
    /// </summary>
    [EnumMember(Value = "chat")]
    Chat
    // По документации других значений пока нет.
}