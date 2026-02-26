using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Responces;

/// <summary>
/// Тип загружаемого файла.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UploadType
{
    [EnumMember(Value = "image")]
    Image,
    [EnumMember(Value = "video")]
    Video,
    [EnumMember(Value = "audio")]
    Audio,
    [EnumMember(Value = "file")]
    File
}

/// <summary>
/// Ответ на запрос получения URL для загрузки файла.
/// </summary>
public record GetUploadUrlResponse
{
    /// <summary>
    /// URL для загрузки файла. Срок жизни ссылки не ограничен.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Токен для видео или аудио, который нужно использовать в сообщении.
    /// Для изображений и файлов может отсутствовать.
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}