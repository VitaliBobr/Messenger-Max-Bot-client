using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.Chat;

public record GetChatsResponse
{
    [JsonPropertyName("chats")]
    public List<Chat> Chats { get; set; } = new();

    [JsonPropertyName("marker")]
    public long? Marker { get; set; }
}