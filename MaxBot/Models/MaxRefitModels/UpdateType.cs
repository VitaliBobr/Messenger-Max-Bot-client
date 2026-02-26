using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum  UpdateType
{
    [EnumMember(Value = "message_created")]
    MessageCreated,
    
    [EnumMember(Value = "message_callback")]
    MessageCallback,
    
    [EnumMember(Value = "message_edited")]
    MessageEdited,
    
    [EnumMember(Value = "message_removed")]
    MessageRemoved,
    
    [EnumMember(Value = "bot_added")]
    BotAdded,
    
    [EnumMember(Value = "bot_removed")]
    BotRemoved,
    
    [EnumMember(Value = "dialog_muted")]
    DialogMuted,
    
    [EnumMember(Value = "dialog_unmuted")]
    DialogUnmuted,
    
    [EnumMember(Value = "dialog_cleared")]
    DialogCleared,
    
    [EnumMember(Value = "dialog_removed")]
    DialogRemoved,
    
    [EnumMember(Value = "user_added")]
    UserAdded,
    
    [EnumMember(Value = "user_removed")]
    UserRemoved,
    
    [EnumMember(Value = "bot_started")]
    BotStarted,
    
    [EnumMember(Value = "bot_stopped")]
    BotStopped,
    
    [EnumMember(Value = "chat_title_changed")]
    ChatTitleChanged,
}