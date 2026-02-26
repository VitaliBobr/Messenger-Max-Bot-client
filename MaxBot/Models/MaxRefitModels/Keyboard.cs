using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels;

/// <summary>
/// RU: Клавиатура (двумерный массив кнопок)
/// ENG: Keyboard (two-dimensional array of buttons)
/// </summary>
public record Keyboard
{
    /// <summary>
    /// RU: Двумерный массив кнопок (ряды и колонки)
    /// ENG: Two-dimensional array of buttons (rows and columns)
    /// </summary>
    [JsonPropertyName("buttons")]
    public List<List<Button>> Buttons { get; set; } = new();
}