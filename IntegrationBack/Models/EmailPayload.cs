using System.Text.Json.Serialization;

public class EmailPayload
{
    [JsonPropertyName("toEmail")]
    public string ToEmail { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}
