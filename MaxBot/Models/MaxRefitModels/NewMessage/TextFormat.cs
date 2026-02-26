using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage;

/// <summary>
/// RU: Формат текста сообщения
/// ENG: Message text format
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TextFormat
{
    /// <summary>Markdown-разметка</summary>
    [EnumMember(Value = "markdown")]
    Markdown,

    /// <summary>HTML-разметка</summary>
    [EnumMember(Value = "html")]
    Html
}