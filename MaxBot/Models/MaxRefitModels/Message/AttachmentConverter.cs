using System.Text.Json;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

namespace MaxBot.Models.MaxRefitModels.Message;

public class AttachmentConverter : JsonConverter<Attachment>
{
    public override Attachment? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement))
            throw new JsonException("Missing 'type' property");
        var typeString = typeElement.GetString();

        if (!root.TryGetProperty("payload", out var payloadElement))
            throw new JsonException("Missing 'payload' property");

        // Вспомогательные функции
        T? GetProp<T>(string propName) where T : class
        {
            if (payloadElement.TryGetProperty(propName, out var propEl))
                return JsonSerializer.Deserialize<T>(propEl.GetRawText(), options);
            return null;
        }

        int? GetInt32(string propName) =>
            payloadElement.TryGetProperty(propName, out var propEl) ? propEl.GetInt32() : null;
        
        long? GetInt64(string propName) =>
            payloadElement.TryGetProperty(propName, out var propEl) ? propEl.GetInt64() : null;
        
        double? GetDouble(string propName) =>
            payloadElement.TryGetProperty(propName, out var propEl) ? propEl.GetDouble() : null;
        
        string? GetString(string propName) =>
            payloadElement.TryGetProperty(propName, out var propEl) ? propEl.GetString() : null;

        Attachment result = typeString switch
        {
            "image" => Attachment.ImageAttachment(
                JsonSerializer.Deserialize<PhotoAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid image payload")
            ),
            "video" => Attachment.VideoAttachment(
                payload: JsonSerializer.Deserialize<MediaAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid video payload"),
                thumbnail: GetProp<VideoThumbnail>("thumbnail")
                    ?? throw new JsonException("Missing thumbnail"),
                width: GetInt32("width"),
                height: GetInt32("height"),
                duration: GetInt32("duration")
            ),
            "audio" => Attachment.AudioAttachment(
                payload: JsonSerializer.Deserialize<MediaAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid audio payload"),
                transcription: GetString("transcription")
            ),
            "file" => Attachment.FileAttachment(
                payload: JsonSerializer.Deserialize<FileAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid file payload"),
                filename: GetString("filename") ?? throw new JsonException("Missing filename"),
                size: GetInt64("size") ?? throw new JsonException("Missing size")
            ),
            "sticker" => Attachment.StickerAttachment(
                payload: JsonSerializer.Deserialize<StickerAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid sticker payload"),
                width: GetInt32("width") ?? throw new JsonException("Missing width"),
                height: GetInt32("height") ?? throw new JsonException("Missing height")
            ),
            "contact" => Attachment.ContactAttachment(
                payload: JsonSerializer.Deserialize<ContactAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid contact payload")
            ),
            "keyboard" => Attachment.KeyboardAttachment(
                payload: JsonSerializer.Deserialize<Keyboard>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid keyboard payload")
            ),
            "share" => Attachment.ShareAttachment(
                payload: JsonSerializer.Deserialize<ShareAttachmentPayload>(payloadElement.GetRawText(), options)
                    ?? throw new JsonException("Invalid share payload"),
                title: GetString("title"),
                description: GetString("description"),
                image_url: GetString("image_url")
            ),
            "location" => Attachment.LocationAttachment(
                latitude: GetDouble("latitude") ?? throw new JsonException("Missing latitude"),
                longitude: GetDouble("longitude") ?? throw new JsonException("Missing longitude")
            ),
            _ => throw new JsonException($"Unknown attachment type: {typeString}")
        };

        // Устанавливаем поле Type в соответствии с полученной строкой
        result.Type = typeString switch
        {
            "image" => AttachmentType.Image,
            "video" => AttachmentType.Video,
            "audio" => AttachmentType.Audio,
            "file" => AttachmentType.File,
            "sticker" => AttachmentType.Sticker,
            "contact" => AttachmentType.Contact,
            "keyboard" => AttachmentType.Keyboard,
            "share" => AttachmentType.Share,
            "location" => AttachmentType.Location,
            _ => throw new JsonException($"Unknown attachment type: {typeString}")
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Attachment value, JsonSerializerOptions options)
    {
        // Используем Match для сериализации, при этом значение Type не используется,
        // так как мы явно пишем строку. Но для единообразия можно было бы использовать
        // value.Type для определения типа, но тогда потребуется преобразование enum->string.
        // Оставляем как есть, с явными строками.
        value.Match(
            imageAttachmentCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "image");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            videoAttachmentCase: (payload, thumbnail, width, height, duration) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "video");
                writer.WriteStartObject("payload");

                // Сериализуем поля из MediaAttachmentPayload
                JsonSerializer.Serialize(writer, payload, options);

                // Добавляем дополнительные поля
                if (thumbnail != null)
                {
                    writer.WritePropertyName("thumbnail");
                    JsonSerializer.Serialize(writer, thumbnail, options);
                }
                if (width.HasValue)
                    writer.WriteNumber("width", width.Value);
                if (height.HasValue)
                    writer.WriteNumber("height", height.Value);
                if (duration.HasValue)
                    writer.WriteNumber("duration", duration.Value);

                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            audioAttachmentCase: (payload, transcription) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "audio");
                writer.WriteStartObject("payload");

                JsonSerializer.Serialize(writer, payload, options);
                if (transcription != null)
                    writer.WriteString("transcription", transcription);

                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            fileAttachmentCase: (payload, filename, size) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "file");
                writer.WriteStartObject("payload");

                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteString("filename", filename);
                writer.WriteNumber("size", size);

                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            stickerAttachmentCase: (payload, width, height) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "sticker");
                writer.WriteStartObject("payload");

                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteNumber("width", width);
                writer.WriteNumber("height", height);

                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            contactAttachmentCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "contact");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            keyboardAttachmentCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "keyboard");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            shareAttachmentCase: (payload, title, description, imageUrl) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "share");
                writer.WriteStartObject("payload");

                JsonSerializer.Serialize(writer, payload, options);
                if (title != null)
                    writer.WriteString("title", title);
                if (description != null)
                    writer.WriteString("description", description);
                if (imageUrl != null)
                    writer.WriteString("image_url", imageUrl);

                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            locationAttachmentCase: (latitude, longitude) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "location");
                writer.WriteStartObject("payload");
                writer.WriteNumber("latitude", latitude);
                writer.WriteNumber("longitude", longitude);
                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            }
        );
    }
}