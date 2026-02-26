using System.Text.Json;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

namespace MaxBot.Models.MaxRefitModels.NewMessage;

public class AttachmentRequestConverter : JsonConverter<AttachmentRequest>
{
    public override AttachmentRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement))
            throw new JsonException("Missing 'type' property");
        var typeString = typeElement.GetString();

        if (!root.TryGetProperty("payload", out var payloadElement))
            throw new JsonException("Missing 'payload' property");

        T? GetPayload<T>() where T : class =>
            JsonSerializer.Deserialize<T>(payloadElement.GetRawText(), options);

        (double lat, double lng) GetLocation()
        {
            if (!payloadElement.TryGetProperty("latitude", out var latEl) ||
                !payloadElement.TryGetProperty("longitude", out var lngEl))
                throw new JsonException("Missing latitude/longitude in location payload");
            return (latEl.GetDouble(), lngEl.GetDouble());
        }

        // 1. Создаём объект через фабричный метод
        AttachmentRequest result = typeString switch
        {
            "image" => AttachmentRequest.ImageAttachmentRequest(
                GetPayload<ImageRequestPayload>() ?? throw new JsonException("Invalid image payload")
            ),
            "video" => AttachmentRequest.VideoAttachmentRequest(
                GetPayload<UploadedInfo>() ?? throw new JsonException("Invalid video payload")
            ),
            "audio" => AttachmentRequest.AudioAttachmentRequest(
                GetPayload<UploadedInfo>() ?? throw new JsonException("Invalid audio payload")
            ),
            "file" => AttachmentRequest.FileAttachmentRequest(
                GetPayload<UploadedInfo>() ?? throw new JsonException("Invalid file payload")
            ),
            "sticker" => AttachmentRequest.StickerAttachmentRequest(
                GetPayload<StickerAttachmentRequestPayload>() ?? throw new JsonException("Invalid sticker payload")
            ),
            "contact" => AttachmentRequest.ContactAttachmentRequest(
                GetPayload<ContactAttachmentRequestPayload>() ?? throw new JsonException("Invalid contact payload")
            ),
            "keyboard" => AttachmentRequest.KeyboardAttachmentRequest(
                GetPayload<InlineKeyboardAttachmentRequestPayload>() ?? throw new JsonException("Invalid keyboard payload")
            ),
            "location" => GetLocation() is var (lat, lng)
                ? AttachmentRequest.LocationAttachmentRequest(lat, lng)
                : throw new JsonException("Invalid location payload"),
            "share" => AttachmentRequest.ShareAttachmentRequest(
                GetPayload<ShareAttachmentPayload>() ?? throw new JsonException("Invalid share payload")
            ),
            _ => throw new JsonException($"Unknown attachment type: {typeString}")
        };

        // 2. Устанавливаем поле Type
        result.Type = typeString switch
        {
            "image" => AttachmentRequestType.Image,
            "video" => AttachmentRequestType.Video,
            "audio" => AttachmentRequestType.Audio,
            "file" => AttachmentRequestType.File,
            "sticker" => AttachmentRequestType.Sticker,
            "contact" => AttachmentRequestType.Contact,
            "keyboard" => AttachmentRequestType.Keyboard,
            "location" => AttachmentRequestType.Location,
            "share" => AttachmentRequestType.Share,
            _ => throw new JsonException($"Unknown attachment type: {typeString}")
        };

        return result;
    }

    public override void Write(Utf8JsonWriter writer, AttachmentRequest value, JsonSerializerOptions options)
    {
        value.Match(
            imageAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "image");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            videoAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "video");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            audioAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "audio");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            fileAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "file");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            stickerAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "sticker");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            contactAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "contact");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            keyboardAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "keyboard");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            },
            locationAttachmentRequestCase: (latitude, longitude) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "location");
                writer.WriteStartObject("payload");
                writer.WriteNumber("latitude", latitude);
                writer.WriteNumber("longitude", longitude);
                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            shareAttachmentRequestCase: (payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "share");
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, payload, options);
                writer.WriteEndObject();
            }
        );
    }
}