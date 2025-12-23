using Microsoft.AspNetCore.Mvc;

namespace YourProjectNamespace.Controllers // Замени на свое пространство имен, если нужно
{
    [ApiController]
    [Route("api/integration")]
    public class IntegrationController : ControllerBase
    {
        private readonly OcrApiService _ocrService;
        private readonly EmailApiService _emailService;

        public IntegrationController(
            OcrApiService ocrService,
            EmailApiService emailService)
        {
            _ocrService = ocrService;
            _emailService = emailService;
        }

        /// <summary>
        /// Проверка связи с бэкендом
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "Integration API is working", time = DateTime.Now });
        }

        /// <summary>
        /// ШАГ 1: Только распознавание текста.
        /// Вызывается первой кнопкой на фронтенде.
        /// </summary>
        [HttpPost("recognize")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Recognize(
            IFormFile image,
            [FromForm] string language = "rus")
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { error = "Изображение не выбрано" });

            try
            {
                // Вызываем твой OCR сервис
                var recognizedText = await _ocrService.RecognizeAsync(image, language);

                return Ok(new
                {
                    success = true,
                    recognizedText = recognizedText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при распознавании", details = ex.Message });
            }
        }

        /// <summary>
        /// ШАГ 2: Только отправка готового (и возможно отредактированного) текста на почту.
        /// Вызывается второй кнопкой на фронтенде.
        /// </summary>
        [HttpPost("send-text")]
        public async Task<IActionResult> SendTextOnly([FromForm] string email, [FromForm] string text)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email обязателен для заполнения" });

            if (string.IsNullOrEmpty(text))
                return BadRequest(new { error = "Текст для отправки пуст" });

            try
            {
                // Вызываем твой Email сервис
                var emailResult = await _emailService.SendAsync(email, text);

                return Ok(new
                {
                    success = true,
                    status = emailResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Ошибка при отправке почты", details = ex.Message });
            }
        }

        /// <summary>
        /// Дополнительный метод: Распознать и отправить сразу (как было раньше)
        /// </summary>
        [HttpPost("process-all")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProcessAll(
            IFormFile image,
            [FromForm] string language,
            [FromForm] string email)
        {
            if (image == null || string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Необходимы и изображение, и email" });

            try
            {
                var text = await _ocrService.RecognizeAsync(image, language);
                var emailResult = await _emailService.SendAsync(email, text);

                return Ok(new IntegrationResponse
                {
                    RecognizedText = text,
                    EmailStatus = emailResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}