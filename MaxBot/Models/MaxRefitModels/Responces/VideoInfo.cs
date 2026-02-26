using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

namespace MaxBot.Models.MaxRefitModels.Responces;


/// <summary>
/// URL-адреса для скачивания или воспроизведения видео.
/// </summary>
public record VideoUrls
{
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Подробная информация о видео.
/// </summary>
public record VideoInfo
{
    /// <summary>
    /// Токен видео-вложения.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// URL-ы для скачивания или воспроизведения видео. Может быть null, если видео недоступно.
    /// </summary>
    [JsonPropertyName("urls")]
    public VideoUrls? Urls { get; set; }

    /// <summary>
    /// Миниатюра видео.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public PhotoAttachmentPayload? Thumbnail { get; set; }

    /// <summary>
    /// Ширина видео в пикселях.
    /// </summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>
    /// Высота видео в пикселях.
    /// </summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }

    /// <summary>
    /// Длительность видео в секундах.
    /// </summary>
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
}