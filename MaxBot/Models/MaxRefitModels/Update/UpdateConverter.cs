using System.Text.Json;
using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Update;

public class UpdateConverter : JsonConverter<Update>
{
    // Статический словарь для сопоставления строковых значений из JSON с членами перечисления
    private static readonly Dictionary<string, UpdateType> UpdateTypeMap = new()
    {
        ["message_created"] = UpdateType.MessageCreated,
        ["message_callback"] = UpdateType.MessageCallback,
        ["message_edited"] = UpdateType.MessageEdited,
        ["message_removed"] = UpdateType.MessageRemoved,
        ["bot_added"] = UpdateType.BotAdded,
        ["bot_removed"] = UpdateType.BotRemoved,
        ["dialog_muted"] = UpdateType.DialogMuted,
        ["dialog_unmuted"] = UpdateType.DialogUnmuted,
        ["dialog_cleared"] = UpdateType.DialogCleared,
        ["dialog_removed"] = UpdateType.DialogRemoved,
        ["user_added"] = UpdateType.UserAdded,
        ["user_removed"] = UpdateType.UserRemoved,
        ["bot_started"] = UpdateType.BotStarted,
        ["bot_stopped"] = UpdateType.BotStopped,
        ["chat_title_changed"] = UpdateType.ChatTitleChanged,
    };

    public override Update? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // 1. Обязательное поле update_type
        if (!root.TryGetProperty("update_type", out var typeElement))
            throw new JsonException("Missing 'update_type' property");

        var typeString = typeElement.GetString();
        // Используем словарь для получения UpdateType
        if (!UpdateTypeMap.TryGetValue(typeString, out var updateType))
            throw new JsonException($"Unknown update type: {typeString}");

        // 2. Обязательное поле timestamp (по документации должно быть)
        if (!root.TryGetProperty("timestamp", out var tsEl))
            throw new JsonException("Missing 'timestamp'");
        var timestamp = tsEl.GetInt64();

        // 3. Вспомогательные методы для безопасного извлечения данных
        T? GetProp<T>(string propName) where T : class
        {
            if (root.TryGetProperty(propName, out var el))
                return JsonSerializer.Deserialize<T>(el.GetRawText(), options);
            return null;
        }

        long GetInt64(string propName) =>
            root.TryGetProperty(propName, out var el) ? el.GetInt64() : throw new JsonException($"Missing {propName}");

        long? GetInt64OrNull(string propName) =>
            root.TryGetProperty(propName, out var el) ? el.GetInt64() : (long?)null;

        string GetString(string propName) =>
            root.TryGetProperty(propName, out var el)
                ? el.GetString()!
                : throw new JsonException($"Missing {propName}");

        string? GetStringOrNull(string propName) =>
            root.TryGetProperty(propName, out var el) ? el.GetString() : null;

        bool GetBool(string propName) =>
            root.TryGetProperty(propName, out var el)
                ? el.GetBoolean()
                : throw new JsonException($"Missing {propName}");

        // 4. Создание конкретного обновления через фабричный метод Dusharp
        Update result = updateType switch
        {
            UpdateType.MessageCreated => Update.MessageCreatedUpdate(
                message: GetProp<Message.Message>("message") ?? throw new JsonException("Missing message"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.MessageCallback => Update.MessageCallbackUpdate(
                callback: GetProp<Callback>("callback") ?? throw new JsonException("Missing callback"),
                message: GetProp<Message.Message>("message"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.MessageEdited => Update.MessageEditedUpdate(
                message: GetProp<Message.Message>("message")
            ),

            UpdateType.MessageRemoved => Update.MessageRemovedUpdate(
                message_id: GetString("message_id"),
                chat_id: GetInt64("chat_id"),
                user_id: GetInt64("user_id")
            ),

            UpdateType.BotAdded => Update.BotAddedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                is_channel: GetBool("is_channel")
            ),

            UpdateType.BotRemoved => Update.BotRemovedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                is_channel: GetBool("is_channel")
            ),

            UpdateType.DialogMuted => Update.DialogMutedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                muted_until: GetInt64("muted_until"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.DialogUnmuted => Update.DialogUnmutedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.DialogCleared => Update.DialogClearedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.DialogRemoved => Update.DialogRemovedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.UserAdded => Update.UserAddedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                inviter_id: GetInt64OrNull("inviter_id"),
                is_channel: GetBool("is_channel")
            ),

            UpdateType.UserRemoved => Update.UserRemovedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                admin_id: GetInt64OrNull("admin_id"),
                is_channel: GetBool("is_channel")
            ),

            UpdateType.BotStarted => Update.BotStartedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                payload: GetStringOrNull("payload"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.BotStopped => Update.BotStoppedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                user_locale: GetStringOrNull("user_locale")
            ),

            UpdateType.ChatTitleChanged => Update.ChatTitleChangedUpdate(
                chat_id: GetInt64("chat_id"),
                user: GetProp<User>("user") ?? throw new JsonException("Missing user"),
                title: GetString("title")
            ),

            _ => throw new JsonException($"Unhandled update type: {updateType}")
        };

        // 5. Устанавливаем общие поля (они не заполняются фабричными методами)
        result.Type = updateType;
        result.Timestamp = timestamp;

        return result;
    }

    public override void Write(Utf8JsonWriter writer, Update value, JsonSerializerOptions options)
    {
        value.Match(
            messageCreatedUpdateCase: (message, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "message_created");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WritePropertyName("message");
                JsonSerializer.Serialize(writer, message, options);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            messageCallbackUpdateCase: (callback, message, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "message_callback");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WritePropertyName("callback");
                JsonSerializer.Serialize(writer, callback, options);
                if (message != null)
                {
                    writer.WritePropertyName("message");
                    JsonSerializer.Serialize(writer, message, options);
                }

                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            messageEditedUpdateCase: (message) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "message_edited");
                writer.WriteNumber("timestamp", value.Timestamp);
                if (message != null)
                {
                    writer.WritePropertyName("message");
                    JsonSerializer.Serialize(writer, message, options);
                }

                writer.WriteEndObject();
            },
            messageRemovedUpdateCase: (messageId, chatId, userId) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "message_removed");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteString("message_id", messageId);
                writer.WriteNumber("chat_id", chatId);
                writer.WriteNumber("user_id", userId);
                writer.WriteEndObject();
            },
            botAddedUpdateCase: (chatId, user, isChannel) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "bot_added");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                writer.WriteBoolean("is_channel", isChannel);
                writer.WriteEndObject();
            },
            botRemovedUpdateCase: (chatId, user, isChannel) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "bot_removed");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                writer.WriteBoolean("is_channel", isChannel);
                writer.WriteEndObject();
            },
            dialogMutedUpdateCase: (chatId, user, mutedUntil, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "dialog_muted");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                writer.WriteNumber("muted_until", mutedUntil);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            dialogUnmutedUpdateCase: (chatId, user, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "dialog_unmuted");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            dialogClearedUpdateCase: (chatId, user, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "dialog_cleared");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            dialogRemovedUpdateCase: (chatId, user, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "dialog_removed");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            userAddedUpdateCase: (chatId, user, inviterId, isChannel) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "user_added");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (inviterId.HasValue)
                    writer.WriteNumber("inviter_id", inviterId.Value);
                writer.WriteBoolean("is_channel", isChannel);
                writer.WriteEndObject();
            },
            userRemovedUpdateCase: (chatId, user, adminId, isChannel) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "user_removed");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (adminId.HasValue)
                    writer.WriteNumber("admin_id", adminId.Value);
                writer.WriteBoolean("is_channel", isChannel);
                writer.WriteEndObject();
            },
            botStartedUpdateCase: (chatId, user, payload, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "bot_started");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (payload != null)
                    writer.WriteString("payload", payload);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            botStoppedUpdateCase: (chatId, user, userLocale) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "bot_stopped");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                if (userLocale != null)
                    writer.WriteString("user_locale", userLocale);
                writer.WriteEndObject();
            },
            chatTitleChangedUpdateCase: (chatId, user, title) =>
            {
                writer.WriteStartObject();
                writer.WriteString("update_type", "chat_title_changed");
                writer.WriteNumber("timestamp", value.Timestamp);
                writer.WriteNumber("chat_id", chatId);
                writer.WritePropertyName("user");
                JsonSerializer.Serialize(writer, user, options);
                writer.WriteString("title", title);
                writer.WriteEndObject();
            }
        );
    }
}