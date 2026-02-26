using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Dusharp;
using Dusharp.Json;

namespace MaxBot.Models.MaxRefitModels;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ButtonType
{
    [EnumMember(Value = "callback")]
    Callback,
    [EnumMember(Value = "link")]
    Link,
    [EnumMember(Value = "request_geo_location")]
    RequestGeoLocation,
    [EnumMember(Value = "open_app")]
    OpenApp,
    [EnumMember(Value = "message")]
    Message
}

[Union]
[GenerateJsonConverter]
public partial class Button
{
    [JsonPropertyName("type")] 
    public ButtonType Type { get; set; }

    
    /// <param name="text">Visible text button</param>
    /// <param name="payload">Button token</param>
    /// <returns></returns>
    [UnionCase]
    public static partial Button CallbackButton(
        string text,
        string payload
        );

    /// <param name="text">Visible text button</param>
    /// <param name="url">url of link</param>
    /// <returns></returns>
    [UnionCase]
    public static partial Button LinkButton(
        string text,
        string url
    );
    
    /// <param name="text">Visible text button</param>
    /// <param name="quick">If true, send geoloc without request to agree user</param>
    /// <returns></returns>
    [UnionCase]
    public static partial Button RequestGeoLocationButton(
        string text,
        bool? quick
    );

    /// <param name="text">Visible text button</param>
    /// <param name="webApp">Публичное имя (username) бота или ссылка на него, чьё мини-приложение надо запустить</param>
    /// <param name="contactId">Идентификатор бота, чьё мини-приложение надо запустить</param>
    /// <param name="payload">Параметр запуска, который будет передан в initData мини-приложения</param>
    /// TODO Replace CamelCase to Snake case into json serializer
    /// <returns></returns>
    [UnionCase]
    public static partial Button OpenAppButton(
        string text,
        string? web_app,
        long? contact_id,
        string? payload
    );
    
    /// <param name="text">Visible text button</param>
    /// TODO Replace CamelCase to Snake case into json serializer
    /// <returns></returns>
    [UnionCase]
    public static partial Button MessageButton(
        string text
    );
    
    
}