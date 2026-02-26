using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Dusharp;
using Dusharp.Json;
using MaxBot.Models.MaxRefitModels.Message;
using MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;
using MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;

namespace MaxBot.Models.MaxRefitModels.NewMessage;

/// <summary>
/// Тип вложения в запросе.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttachmentRequestType
{
    [EnumMember(Value = "image")]
    Image,
    
    [EnumMember(Value = "video")]
    Video,
    
    [EnumMember(Value = "audio")]
    Audio,
    
    [EnumMember(Value = "file")]
    File,
    
    [EnumMember(Value = "sticker")]
    Sticker,
    
    [EnumMember(Value = "contact")]
    Contact,
    
    [EnumMember(Value = "keyboard")]
    Keyboard,
    
    [EnumMember(Value = "location")]
    Location,
    
    [EnumMember(Value = "share")]
    Share
}

/// <summary>
/// RU: Вложение сообщения. Всегда содержит type и payload
/// ENG: Message attachment. Always contains type and payload
/// </summary>
[Union]
[GenerateJsonConverter]
public partial class AttachmentRequest
{
    [JsonPropertyName("type")]
    public AttachmentRequestType Type { get; set; }

    [UnionCase]
    public static partial AttachmentRequest ImageAttachmentRequest(ImageRequestPayload payload);
    
    
    [UnionCase]
    public static partial AttachmentRequest VideoAttachmentRequest(UploadedInfo payload);
    
    [UnionCase]
    public static partial AttachmentRequest AudioAttachmentRequest(UploadedInfo payload);
    
    [UnionCase]
    public static partial AttachmentRequest FileAttachmentRequest(UploadedInfo payload);
    
    [UnionCase]
    public static partial AttachmentRequest StickerAttachmentRequest(StickerAttachmentRequestPayload payload);
    
    [UnionCase]
    public static partial AttachmentRequest ContactAttachmentRequest(ContactAttachmentRequestPayload payload);
    
    [UnionCase]
    public static partial AttachmentRequest KeyboardAttachmentRequest(InlineKeyboardAttachmentRequestPayload payload);
    
    [UnionCase]
    public static partial AttachmentRequest LocationAttachmentRequest(
        double latitude,
        double longitude
    );
    
    [UnionCase]
    public static partial AttachmentRequest ShareAttachmentRequest(
        ShareAttachmentPayload payload
    );
}