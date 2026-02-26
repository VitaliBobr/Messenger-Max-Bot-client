using System.Text.Json.Serialization;
using Dusharp;
using Dusharp.Json;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

/// <summary>
/// RU: Запрос на прикрепление изображения
/// ENG: Image attachment request
/// </summary>
[Union]
[GenerateJsonConverter]
public partial class ImageRequestPayload
{
    /// <summary>
    /// RU: Изображение по URL
    /// ENG: Image from URL
    /// </summary>
    [UnionCase]
    public static partial ImageRequestPayload FromUrl(
        string url
    );
    
    /// <summary>
    /// RU: Изображение по токену
    /// ENG: Image from token
    /// </summary>
    [UnionCase]
    public static partial ImageRequestPayload FromToken(
        string token
    );
    
    /// <summary>
    /// RU: Изображение из загруженных фото
    /// ENG: Image from uploaded photos
    /// </summary>
    [UnionCase]
    public static partial ImageRequestPayload FromPhotos(
         UploadedPhotos Photos
    );
}

/// <summary>
/// RU: Загруженные фото
/// ENG: Uploaded photos
/// </summary>
public record UploadedPhotos
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}