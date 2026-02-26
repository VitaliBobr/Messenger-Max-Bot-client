using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels;

/// <summary>
/// RU: Перечень прав администратора в чате
/// ENG: List of administrator permissions in chat
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatAdminPermission
{
    /// <summary>Читать все сообщения / Read all messages</summary>
    [EnumMember(Value = "read_all_messages")]
    ReadAllMessages,

    /// <summary>Добавлять/удалять участников / Add or remove members</summary>
    [EnumMember(Value = "add_remove_members")]
    AddRemoveMembers,

    /// <summary>Добавлять администраторов / Add administrators</summary>
    [EnumMember(Value = "add_admins")]
    AddAdmins,

    /// <summary>Изменять информацию о чате / Change chat info</summary>
    [EnumMember(Value = "change_chat_info")]
    ChangeChatInfo,

    /// <summary>Закреплять сообщения / Pin messages</summary>
    [EnumMember(Value = "pin_message")]
    PinMessage,

    /// <summary>Писать сообщения / Write messages</summary>
    [EnumMember(Value = "write")]
    Write,

    /// <summary>Совершать звонки / Can call</summary>
    [EnumMember(Value = "can_call")]
    CanCall,

    /// <summary>Изменять ссылку на чат / Edit chat link</summary>
    [EnumMember(Value = "edit_link")]
    EditLink,

    /// <summary>Публиковать, редактировать, удалять сообщения / Post, edit, delete messages</summary>
    [EnumMember(Value = "post_edit_delete_message")]
    PostEditDeleteMessage,

    /// <summary>Редактировать сообщения / Edit messages</summary>
    [EnumMember(Value = "edit_message")]
    EditMessage,

    /// <summary>Удалять сообщения / Delete messages</summary>
    [EnumMember(Value = "delete_message")]
    DeleteMessage
}