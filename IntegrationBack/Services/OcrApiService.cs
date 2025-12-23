using System.Net.Http.Headers;
using System.Text.Json;

public class OcrApiService
{
    private readonly HttpClient _httpClient;
    private const string OcrUrl = "http://31.57.28.121:5000/api/imgtotext";

    public OcrApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> RecognizeAsync(IFormFile image, string language)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("Image file is required");

        using var content = new MultipartFormDataContent();

        // Содержимое файла
        var fileContent = new StreamContent(image.OpenReadStream());
        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(image.ContentType ?? "application/octet-stream");

        // ⚠️ Имя поля точно "Image" (чувствительно к регистру)
        content.Add(fileContent, "Image", image.FileName);

        // Язык
        content.Add(new StringContent(language), "language");

        // Отправка запроса
        var response = await _httpClient.PostAsync(OcrUrl, content);

        // Читаем тело ответа для отладки
        var responseBody = await response.Content.ReadAsStringAsync();

        // Проверка кода ответа
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"OCR API error\nStatus: {(int)response.StatusCode}\nResponse: {responseBody}"
            );
        }

        // 🔹 Десериализация JSON и возврат только recognizedText
        try
        {
            var jsonDoc = JsonDocument.Parse(responseBody);
            if (jsonDoc.RootElement.TryGetProperty("recognizedText", out var recognizedText))
            {
                return recognizedText.GetString()?.Trim();
            }
            else
            {
                // Если структура неожиданная, возвращаем весь ответ
                return responseBody;
            }
        }
        catch
        {
            // Если не JSON, возвращаем как есть
            return responseBody;
        }
    }
}
