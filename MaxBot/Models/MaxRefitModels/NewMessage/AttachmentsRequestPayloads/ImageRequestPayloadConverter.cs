using System.Text.Json;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

public class ImageRequestPayloadConverter : JsonConverter<ImageRequestPayload>
{
    public override ImageRequestPayload? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // Проверяем наличие поля url
        if (root.TryGetProperty("url", out var urlEl))
        {
            var url = urlEl.GetString();
            if (string.IsNullOrEmpty(url))
                throw new JsonException("url cannot be empty");
            return ImageRequestPayload.FromUrl(url);
        }

        // Проверяем наличие поля token
        if (root.TryGetProperty("token", out var tokenEl))
        {
            var token = tokenEl.GetString();
            if (string.IsNullOrEmpty(token))
                throw new JsonException("token cannot be empty");
            return ImageRequestPayload.FromToken(token);
        }

        // Проверяем наличие поля photos (объект)
        if (root.TryGetProperty("photos", out var photosEl))
        {
            var photos = JsonSerializer.Deserialize<UploadedPhotos>(photosEl.GetRawText(), options);
            if (photos == null || string.IsNullOrEmpty(photos.Token))
                throw new JsonException("photos object must contain a valid token");
            return ImageRequestPayload.FromPhotos(photos);
        }

        throw new JsonException("No valid image request payload found. Expected 'url', 'token', or 'photos' field.");
    }

    public override void Write(Utf8JsonWriter writer, ImageRequestPayload value, JsonSerializerOptions options)
    {
        value.Match(
            fromUrlCase: (url) =>
            {
                writer.WriteStartObject();
                writer.WriteString("url", url);
                writer.WriteEndObject();
            },
            fromTokenCase: (token) =>
            {
                writer.WriteStartObject();
                writer.WriteString("token", token);
                writer.WriteEndObject();
            },
            fromPhotosCase: (photos) =>
            {
                writer.WriteStartObject();
                writer.WritePropertyName("photos");
                JsonSerializer.Serialize(writer, photos, options);
                writer.WriteEndObject();
            }
        );
    }
}