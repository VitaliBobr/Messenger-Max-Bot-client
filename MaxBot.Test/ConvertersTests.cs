using System.Text.Json;
using MaxBot.Models.MaxRefitModels;
using MaxBot.Models.MaxRefitModels.Message;
using MaxBot.Models.MaxRefitModels.NewMessage;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;
using MaxBot.Models.MaxRefitModels.Update;

namespace MaxBot.Test;

// ==================== UpdateConverterTests ====================
public class UpdateConverterTests
{
    private readonly JsonSerializerOptions _options;

    public UpdateConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new UpdateConverter() }
        };
    }

    [Fact]
    public void Deserialize_MessageCreatedUpdate_ShouldReturnCorrectObject()
    {
        var json = "{\"update_type\":\"message_created\",\"timestamp\":123456789,\"message\":{\"sender\":{\"user_id\":123,\"first_name\":\"Alice\"},\"recipient\":{\"chat_id\":456,\"chat_type\":\"chat\"},\"timestamp\":123456789,\"body\":{\"mid\":\"msg_123\",\"seq\":42,\"text\":\"Hello, world!\"}},\"user_locale\":\"ru\"}";
        var update = JsonSerializer.Deserialize<Update>(json, _options);

        Assert.NotNull(update);
        update!.Match(
            messageCreatedUpdateCase: (msg, locale) =>
            {
                Assert.Equal("ru", locale);
                Assert.NotNull(msg.Body);
                Assert.Equal("msg_123", msg.Body.Mid);
                Assert.Equal(42, msg.Body.Seq);
                Assert.Equal("Hello, world!", msg.Body.Text);
            },
            messageCallbackUpdateCase: (_, _, _) => Assert.Fail("Unexpected message callback"),
            messageEditedUpdateCase: _ => Assert.Fail("Unexpected message edited"),
            messageRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected message removed"),
            botAddedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot added"),
            botRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot removed"),
            dialogMutedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected dialog muted"),
            dialogUnmutedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog unmuted"),
            dialogClearedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog cleared"),
            dialogRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog removed"),
            userAddedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user added"),
            userRemovedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user removed"),
            botStartedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected bot started"),
            botStoppedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot stopped"),
            chatTitleChangedUpdateCase: (_, _, _) => Assert.Fail("Unexpected chat title changed")
        );
    }

    [Fact]
    public void Deserialize_MessageCallbackUpdate_ShouldReturnCorrectObject()
    {
        // Arrange
        var json = @"
        {
            ""update_type"": ""message_callback"",
            ""timestamp"": 123456789,
            ""callback"": {
                ""timestamp"": 123456789,
                ""callback_id"": ""keyboard_123"",
                ""payload"": ""button_data"",
                ""user"": {
                    ""user_id"": 42,
                    ""first_name"": ""Ivan"",
                    ""is_bot"": false
                }
            }
        }";

        // Act
        var update = JsonSerializer.Deserialize<Update>(json, _options);

        // Assert
        Assert.NotNull(update);
        update!.Match(
            messageCallbackUpdateCase: (callback, message, userLocale) =>
            {
                Assert.Equal(123456789, callback.Timestamp);
                Assert.Equal("keyboard_123", callback.CallbackId);
                Assert.Equal("button_data", callback.Payload);
                Assert.NotNull(callback.User);
                Assert.Equal(42, callback.User.UserId);
                Assert.Equal("Ivan", callback.User.FirstName);
                Assert.False(callback.User.IsBot);
                Assert.Null(message);
                Assert.Null(userLocale);
            },
            messageCreatedUpdateCase: (_, _) => Assert.Fail("Unexpected message created"),
            messageEditedUpdateCase: _ => Assert.Fail("Unexpected message edited"),
            messageRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected message removed"),
            botAddedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot added"),
            botRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot removed"),
            dialogMutedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected dialog muted"),
            dialogUnmutedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog unmuted"),
            dialogClearedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog cleared"),
            dialogRemovedUpdateCase: (_, _, _) => Assert.Fail("Unexpected dialog removed"),
            userAddedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user added"),
            userRemovedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected user removed"),
            botStartedUpdateCase: (_, _, _, _) => Assert.Fail("Unexpected bot started"),
            botStoppedUpdateCase: (_, _, _) => Assert.Fail("Unexpected bot stopped"),
            chatTitleChangedUpdateCase: (_, _, _) => Assert.Fail("Unexpected chat title changed")
        );
    }

    // Аналогично для остальных типов...
}

// ==================== AttachmentRequestConverterTests ====================
public class AttachmentRequestConverterTests
{
    private readonly JsonSerializerOptions _options;

    public AttachmentRequestConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters =
            {
                new AttachmentRequestConverter(),
                new ImageRequestPayloadConverter()
            }
        };
    }

    [Fact]
    public void Deserialize_ImageAttachmentRequest_FromUrl_ShouldReturnCorrectObject()
    {
        var json = "{\"type\":\"image\",\"payload\":{\"url\":\"http://example.com/img.jpg\"}}";
        var req = JsonSerializer.Deserialize<AttachmentRequest>(json, _options);

        Assert.NotNull(req);
        req!.Match(
            imageAttachmentRequestCase: payload => payload.Match(
                fromUrlCase: url => Assert.Equal("http://example.com/img.jpg", url),
                fromTokenCase: _ => Assert.Fail("Expected fromUrl, got fromToken"),
                fromPhotosCase: _ => Assert.Fail("Expected fromUrl, got fromPhotos")
            ),
            videoAttachmentRequestCase: _ => Assert.Fail("Unexpected video attachment"),
            audioAttachmentRequestCase: _ => Assert.Fail("Unexpected audio attachment"),
            fileAttachmentRequestCase: _ => Assert.Fail("Unexpected file attachment"),
            stickerAttachmentRequestCase: _ => Assert.Fail("Unexpected sticker attachment"),
            contactAttachmentRequestCase: _ => Assert.Fail("Unexpected contact attachment"),
            keyboardAttachmentRequestCase: _ => Assert.Fail("Unexpected keyboard attachment"),
            locationAttachmentRequestCase: (_, _) => Assert.Fail("Unexpected location attachment"),
            shareAttachmentRequestCase: _ => Assert.Fail("Unexpected share attachment")
        );
    }

    [Fact]
    public void Deserialize_VideoAttachmentRequest_ShouldReturnCorrectObject()
    {
        var json = "{\"type\":\"video\",\"payload\":{\"token\":\"vid_token\"}}";
        var req = JsonSerializer.Deserialize<AttachmentRequest>(json, _options);

        Assert.NotNull(req);
        req!.Match(
            videoAttachmentRequestCase: info => Assert.Equal("vid_token", info.Token),
            imageAttachmentRequestCase: _ => Assert.Fail("Unexpected image attachment"),
            audioAttachmentRequestCase: _ => Assert.Fail("Unexpected audio attachment"),
            fileAttachmentRequestCase: _ => Assert.Fail("Unexpected file attachment"),
            stickerAttachmentRequestCase: _ => Assert.Fail("Unexpected sticker attachment"),
            contactAttachmentRequestCase: _ => Assert.Fail("Unexpected contact attachment"),
            keyboardAttachmentRequestCase: _ => Assert.Fail("Unexpected keyboard attachment"),
            locationAttachmentRequestCase: (_, _) => Assert.Fail("Unexpected location attachment"),
            shareAttachmentRequestCase: _ => Assert.Fail("Unexpected share attachment")
        );
    }

    // Аналогично для остальных типов...
}

// ==================== AttachmentConverterTests ====================
public class AttachmentConverterTests
{
    private readonly JsonSerializerOptions _options;

    public AttachmentConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new AttachmentConverter() }
        };
    }

    [Fact]
    public void Deserialize_ImageAttachment_ShouldReturnCorrectObject()
    {
        var json = "{\"type\":\"image\",\"payload\":{\"photo_id\":123,\"token\":\"t\",\"url\":\"u\"}}";
        var att = JsonSerializer.Deserialize<Attachment>(json, _options);

        Assert.NotNull(att);
        att!.Match(
            imageAttachmentCase: p =>
            {
                Assert.Equal(123, p.PhotoId);
                Assert.Equal("t", p.Token);
                Assert.Equal("u", p.Url);
            },
            videoAttachmentCase: (_, _, _, _, _) => Assert.Fail("Unexpected video attachment"),
            audioAttachmentCase: (_, _) => Assert.Fail("Unexpected audio attachment"),
            fileAttachmentCase: (_, _, _) => Assert.Fail("Unexpected file attachment"),
            stickerAttachmentCase: (_, _, _) => Assert.Fail("Unexpected sticker attachment"),
            contactAttachmentCase: _ => Assert.Fail("Unexpected contact attachment"),
            keyboardAttachmentCase: _ => Assert.Fail("Unexpected keyboard attachment"),
            shareAttachmentCase: (_, _, _, _) => Assert.Fail("Unexpected share attachment"),
            locationAttachmentCase: (_, _) => Assert.Fail("Unexpected location attachment")
        );
    }

    // Аналогично для остальных типов...
}

// ==================== ButtonConverterTests ====================
public class ButtonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public ButtonConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new ButtonConverter() }
        };
    }

    [Fact]
    public void Deserialize_CallbackButton_ShouldReturnCorrectObject()
    {
        var json = "{\"type\":\"callback\",\"payload\":{\"text\":\"Click\",\"payload\":\"data\"}}";
        var btn = JsonSerializer.Deserialize<Button>(json, _options);

        Assert.NotNull(btn);
        btn!.Match(
            callbackButtonCase: (text, payload) =>
            {
                Assert.Equal("Click", text);
                Assert.Equal("data", payload);
            },
            linkButtonCase: (_, _) => Assert.Fail("Unexpected link button"),
            requestGeoLocationButtonCase: (_, _) => Assert.Fail("Unexpected request geo button"),
            openAppButtonCase: (_, _, _, _) => Assert.Fail("Unexpected open app button"),
            messageButtonCase: _ => Assert.Fail("Unexpected message button")
        );
    }

    [Fact]
    public void Deserialize_LinkButton_ShouldReturnCorrectObject()
    {
        var json = "{\"type\":\"link\",\"payload\":{\"text\":\"Open\",\"url\":\"https://example.com\"}}";
        var btn = JsonSerializer.Deserialize<Button>(json, _options);

        Assert.NotNull(btn);
        btn!.Match(
            linkButtonCase: (text, url) =>
            {
                Assert.Equal("Open", text);
                Assert.Equal("https://example.com", url);
            },
            callbackButtonCase: (_, _) => Assert.Fail("Unexpected callback button"),
            requestGeoLocationButtonCase: (_, _) => Assert.Fail("Unexpected request geo button"),
            openAppButtonCase: (_, _, _, _) => Assert.Fail("Unexpected open app button"),
            messageButtonCase: _ => Assert.Fail("Unexpected message button")
        );
    }

    // Аналогично для остальных типов...
}

// ==================== ImageRequestPayloadConverterTests ====================
public class ImageRequestPayloadConverterTests
{
    private readonly JsonSerializerOptions _options;

    public ImageRequestPayloadConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new ImageRequestPayloadConverter() }
        };
    }

    [Fact]
    public void Deserialize_FromUrl_ShouldReturnCorrectPayload()
    {
        var json = "{\"url\":\"http://example.com/i.jpg\"}";
        var payload = JsonSerializer.Deserialize<ImageRequestPayload>(json, _options);

        Assert.NotNull(payload);
        payload!.Match(
            fromUrlCase: url => Assert.Equal("http://example.com/i.jpg", url),
            fromTokenCase: _ => Assert.Fail("Expected fromUrl, got fromToken"),
            fromPhotosCase: _ => Assert.Fail("Expected fromUrl, got fromPhotos")
        );
    }

    [Fact]
    public void Deserialize_FromToken_ShouldReturnCorrectPayload()
    {
        var json = "{\"token\":\"token123\"}";
        var payload = JsonSerializer.Deserialize<ImageRequestPayload>(json, _options);

        Assert.NotNull(payload);
        payload!.Match(
            fromTokenCase: token => Assert.Equal("token123", token),
            fromUrlCase: _ => Assert.Fail("Expected fromToken, got fromUrl"),
            fromPhotosCase: _ => Assert.Fail("Expected fromToken, got fromPhotos")
        );
    }

    [Fact]
    public void Deserialize_FromPhotos_ShouldReturnCorrectPayload()
    {
        var json = "{\"photos\":{\"token\":\"photo_token\"}}";
        var payload = JsonSerializer.Deserialize<ImageRequestPayload>(json, _options);

        Assert.NotNull(payload);
        payload!.Match(
            fromPhotosCase: photos => Assert.Equal("photo_token", photos.Token),
            fromUrlCase: _ => Assert.Fail("Expected fromPhotos, got fromUrl"),
            fromTokenCase: _ => Assert.Fail("Expected fromPhotos, got fromToken")
        );
    }
}