using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EmailApiService
{
    private readonly HttpClient _httpClient;
    private const string EmailUrl = "http://apisendemail-oribon.amvera.io/sendEmail";

    public EmailApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendAsync(string email, string text)
    {
        var payload = new EmailPayload
        {
            ToEmail = email,
            Subject = "OCR result",
            Text = text
        };

        var json = JsonSerializer.Serialize(payload);

        // 🔍 Логируем для отладки
        Console.WriteLine("JSON to send:");
        Console.WriteLine(json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(EmailUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
            return "Email sent successfully (201)";

        throw new Exception(
            $"Email API error\nStatus: {(int)response.StatusCode}\nResponse: {responseBody}"
        );
    }
}
