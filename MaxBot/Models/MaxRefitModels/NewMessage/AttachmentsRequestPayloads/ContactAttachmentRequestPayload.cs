using System.Text.Json.Serialization;

namespace MaxBot.Models.MaxRefitModels.NewMessage.AttachmentsRequestPayloads;


/// <summary>
/// RU: Запрос на прикрепление контакта
/// ENG: Contact attachment request payload
/// </summary>
public record ContactAttachmentRequestPayload
{
    /// <summary>
    /// RU: Имя контакта
    /// ENG: Contact name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// RU: ID контакта в MAX (если зарегистрирован)
    /// ENG: Contact ID in MAX (if registered)
    /// </summary>
    [JsonPropertyName("contact_id")]
    public long? ContactId { get; set; }

    /// <summary>
    /// RU: Полная информация в формате VCF
    /// ENG: Full contact info in VCF format
    /// </summary>
    [JsonPropertyName("vcf_info")]
    public string? VcfInfo { get; set; }

    /// <summary>
    /// RU: Телефон контакта в формате VCF
    /// ENG: Contact phone in VCF format
    /// </summary>
    [JsonPropertyName("vcf_phone")]
    public string? VcfPhone { get; set; }
}