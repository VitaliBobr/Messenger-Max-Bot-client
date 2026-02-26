using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Dusharp;
using Dusharp.Json;
using MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;


namespace MaxBot.Models.MaxRefitModels.Message;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttachmentType
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
    [EnumMember(Value = "share")]
    Share,
    [EnumMember(Value = "location")]
    Location
}

/// <summary>
/// RU: Вложение сообщения. Всегда содержит type и payload
/// ENG: Message attachment. Always contains type and payload
/// </summary>
[Union]
[GenerateJsonConverter] 
public partial class Attachment
{
    [JsonPropertyName("type")]
    public AttachmentType Type { get; set; }

    [UnionCase]
    public static partial Attachment ImageAttachment(PhotoAttachmentPayload payload);

    [UnionCase]
    public static partial Attachment VideoAttachment(
        MediaAttachmentPayload payload,
        VideoThumbnail thumbnail,
        int? width,
        int? height,
        int? duration
    );
    
    [UnionCase]
    public static partial Attachment AudioAttachment(
        MediaAttachmentPayload payload,
        string? transcription
    );
    
    [UnionCase]
    public static partial Attachment FileAttachment(
        FileAttachmentPayload payload,
        string filename,
        long size
    );
    
    [UnionCase]
    public static partial Attachment StickerAttachment(
        StickerAttachmentPayload payload,
        int width,
        int height
    );
    
    [UnionCase]
    public static partial Attachment ContactAttachment(
        ContactAttachmentPayload payload
    );
    
    [UnionCase]
    public static partial Attachment KeyboardAttachment(
        Keyboard payload
    );
    
    [UnionCase]
    public static partial Attachment ShareAttachment(
        ShareAttachmentPayload payload,
        string? title,
        string? description,
        string? image_url
    );
    
    [UnionCase]
    public static partial Attachment LocationAttachment(
        double latitude,
        double longitude
    );
    
}