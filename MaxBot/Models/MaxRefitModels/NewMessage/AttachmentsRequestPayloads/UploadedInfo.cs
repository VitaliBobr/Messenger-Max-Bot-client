using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

/// <summary>
/// RU: Информация о загруженном медиафайле
/// ENG: Uploaded media file information
/// </summary>
public record UploadedInfo
{
    /// <summary>
    /// RU: Токен — уникальный ID загруженного медиафайла
    /// ENG: Token — unique ID of uploaded media file
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}