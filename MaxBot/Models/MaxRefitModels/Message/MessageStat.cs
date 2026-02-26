using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Message;

/// <summary>
/// RU: Статистика сообщения
/// ENG: Message statistics
/// </summary>
public record MessageStat
{
    /// <summary>
    /// RU: Количество просмотров (для каналов)
    /// ENG: Views count (for channels)
    /// </summary>
    [JsonPropertyName("views")]
    public int Views { get; set; }
}