using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Requests;

/// <summary>
/// Действие, отправляемое участникам чата.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SenderAction
{
    [EnumMember(Value = "typing_on")]
    TypingOn,
    
    [EnumMember(Value = "sending_photo")]
    SendingPhoto,
    
    [EnumMember(Value = "sending_video")]
    SendingVideo,
    
    [EnumMember(Value = "sending_audio")]
    SendingAudio,
    
    [EnumMember(Value = "sending_file")]
    SendingFile,
    
    [EnumMember(Value = "mark_seen")]
    MarkSeen,
    /// <summary>
    /// Only for tests for testing invariant
    /// No into api
    /// </summary>
    [EnumMember(Value = "invalid_action")]
    InvalidAction,
}

/// <summary>
/// Модель запроса для отправки действия бота в чат.
/// </summary>
public record SenderActionRequest
{
    /// <summary>
    /// Действие, отправляемое участникам чата.
    /// </summary>
    [JsonPropertyName("action")]
    public SenderAction Action { get; set; }
}