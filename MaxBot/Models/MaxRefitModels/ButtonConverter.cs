using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels;

public class ButtonConverter : JsonConverter<Button>
{
    public override Button? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement))
            throw new JsonException("Missing 'type' property");
        var typeString = typeElement.GetString();

        if (!root.TryGetProperty("payload", out var payloadElement))
            throw new JsonException("Missing 'payload' property");

        // 👇 Вспомогательные локальные функции
        string GetString(string propName) =>
            payloadElement.TryGetProperty(propName, out var prop) ? prop.GetString()!
                : throw new JsonException($"Missing '{propName}' in payload");

        string? GetStringOrNull(string propName) =>
            payloadElement.TryGetProperty(propName, out var prop) ? prop.GetString() : null;

        long? GetInt64OrNull(string propName) =>
            payloadElement.TryGetProperty(propName, out var prop) && prop.ValueKind != JsonValueKind.Null
                ? prop.GetInt64()
                : (long?)null;

        bool? GetBooleanOrNull(string propName) =>
            payloadElement.TryGetProperty(propName, out var prop) && prop.ValueKind != JsonValueKind.Null
                ? prop.GetBoolean()
                : (bool?)null;
        
        
        // Вспомогательные функции (как у вас) ...

        Button result = typeString switch
        {
            "callback" => Button.CallbackButton(
                text: GetString("text"),
                payload: GetString("payload")
            ),
            "link" => Button.LinkButton(
                text: GetString("text"),
                url: GetString("url")
            ),
            "request_geo_location" => Button.RequestGeoLocationButton(
                text: GetString("text"),
                quick: GetBooleanOrNull("quick")
            ),
            "open_app" => Button.OpenAppButton(
                text: GetString("text"),
                web_app: GetStringOrNull("web_app"),
                contact_id: GetInt64OrNull("contact_id"),
                payload: GetStringOrNull("payload")
            ),
            "message" => Button.MessageButton(
                text: GetString("text")
            ),
            _ => throw new JsonException($"Unknown button type: {typeString}")
        };

        // Устанавливаем Type на основе строки
        result.Type = typeString switch
        {
            "callback" => ButtonType.Callback,
            "link" => ButtonType.Link,
            "request_geo_location" => ButtonType.RequestGeoLocation,
            "open_app" => ButtonType.OpenApp,
            "message" => ButtonType.Message,
            _ => throw new JsonException($"Unknown button type: {typeString}")
        };

        return result;
    }
    
    public override void Write(Utf8JsonWriter writer, Button value, JsonSerializerOptions options)
    {
        value.Match(
            callbackButtonCase: (text, payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "callback");
                writer.WriteStartObject("payload");
                writer.WriteString("text", text);
                writer.WriteString("payload", payload);
                writer.WriteEndObject(); // payload
                writer.WriteEndObject(); // root
            },
            linkButtonCase: (text, url) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "link");
                writer.WriteStartObject("payload");
                writer.WriteString("text", text);
                writer.WriteString("url", url);
                writer.WriteEndObject();
                writer.WriteEndObject();
            },
            requestGeoLocationButtonCase: (text, quick) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "request_geo_location");
                writer.WriteStartObject("payload");
                writer.WriteString("text", text);
                if (quick.HasValue)
                    writer.WriteBoolean("quick", quick.Value);
                writer.WriteEndObject();
                writer.WriteEndObject();
            },
            openAppButtonCase: (text, webApp, contactId, payload) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "open_app");
                writer.WriteStartObject("payload");
                writer.WriteString("text", text);
                if (webApp != null)
                    writer.WriteString("web_app", webApp);
                if (contactId.HasValue)
                    writer.WriteNumber("contact_id", contactId.Value);
                if (payload != null)
                    writer.WriteString("payload", payload);
                writer.WriteEndObject();
                writer.WriteEndObject();
            },
            messageButtonCase: (text) =>
            {
                writer.WriteStartObject();
                writer.WriteString("type", "message");
                writer.WriteStartObject("payload");
                writer.WriteString("text", text);
                writer.WriteEndObject();
                writer.WriteEndObject();
            }
        );
    }
}