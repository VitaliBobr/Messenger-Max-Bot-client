using System.Text.Json.Serialization;
using Dusharp;

namespace MaxBot.Models.MaxRefitModels.Message;

[Union]
public partial class MarkupElement
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [UnionCase]
    public static partial MarkupElement StrongMarkup(
        int from,
        int length
        );
    
    [UnionCase]
    public static partial MarkupElement EmphasizedMarkup(
        int from,
        int length
    );
    
    [UnionCase]
    public static partial MarkupElement MonospacedMarkup(
        int from,
        int length
    );
    
    [UnionCase]
    public static partial MarkupElement LinkMarkup(
        int from,
        int length,
        string link
    );
    
    [UnionCase]
    public static partial MarkupElement StrikethroughMarkup(
        int from,
        int length
    );
    
    [UnionCase]
    public static partial MarkupElement UnderlineMarkup(
        int from,
        int length
    );
    
    [UnionCase]
    public static partial MarkupElement UserMentionMarkup(
        int from,
        int length,
        string? user_link,
        long? user_id
    );
}