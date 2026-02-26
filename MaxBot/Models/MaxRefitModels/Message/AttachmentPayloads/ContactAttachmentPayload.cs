using System.Text.Json.Serialization;
using MaxBot.Models.MaxRefitModels.Users;

namespace MaxBot.Models.MaxRefitModels.Message.AttachmentPayloads;

/// <summary>
/// RU: Данные контакта
/// ENG: Contact attachment payload
/// </summary>
public record ContactAttachmentPayload
{
    /// <summary>
    /// RU: Информация о пользователе в формате VCF
    /// ENG: User information in VCF format
    /// </summary>
    [JsonPropertyName("vcf_info")]
    public string? VcfInfo { get; set; }

    /// <summary>
    /// RU: Информация о пользователе MAX
    /// ENG: MAX user information
    /// </summary>
    [JsonPropertyName("max_info")]
    public User? MaxInfo { get; set; }
}