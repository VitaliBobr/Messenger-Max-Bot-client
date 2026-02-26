using System.Text.Json.Serialization;
using Dusharp;
using Dusharp.Json;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Update;

/// <summary>
/// RU: Базовый класс для всех типов обновлений
/// ENG: Base class for all update types
/// </summary>
[Union]
[GenerateJsonConverter]
public partial class Update
{
    /// <summary>
    /// RU: Тип обновления
    /// ENG: Update type
    /// </summary>
    [JsonPropertyName("update_type")]
    public UpdateType Type { get; set; }

    /// <summary>
    /// RU: Время события (Unix ms)
    /// ENG: Event time (Unix ms)
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    
    [UnionCase]
    public static partial Update MessageCreatedUpdate( 
        Message.Message message,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update MessageCallbackUpdate( 
        Callback callback,
        Message.Message? message,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update MessageEditedUpdate( 
        Message.Message? message
    );
    
    [UnionCase]
    public static partial Update MessageRemovedUpdate( 
        string message_id,
        long chat_id,
        long user_id
    );
    
    [UnionCase]
    public static partial Update BotAddedUpdate( 
        long chat_id,
        User user,
        bool is_channel
    );
    
    [UnionCase]
    public static partial Update BotRemovedUpdate( 
        long chat_id,
        User user,
        bool is_channel
    );
    
    [UnionCase]
    public static partial Update DialogMutedUpdate( 
        long chat_id,
        User user,
        long muted_until,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update DialogUnmutedUpdate( 
        long chat_id,
        User user,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update DialogClearedUpdate( 
        long chat_id,
        User user,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update DialogRemovedUpdate( 
        long chat_id,
        User user,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update UserAddedUpdate( 
        long chat_id,
        User user,
        long? inviter_id,
        bool is_channel
    );
    
    [UnionCase]
    public static partial Update UserRemovedUpdate( 
        long chat_id,
        User user,
        long? admin_id,
        bool is_channel
    );
    
    [UnionCase]
    public static partial Update BotStartedUpdate( 
        long chat_id,
        User user,
        string? payload,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update BotStoppedUpdate( 
        long chat_id,
        User user,
        string? user_locale
    );
    
    [UnionCase]
    public static partial Update ChatTitleChangedUpdate( 
        long chat_id,
        User user,
        string title
    );
}