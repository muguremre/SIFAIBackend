using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TumorController : ControllerBase
    {
        private readonly string flaskApiUrl = "http://127.0.0.1:5000/predict"; // Flask API URL

        [HttpPost("detect")]
        public async Task<IActionResult> DetectTumor([FromForm] TumorDetectionRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
            {
                return BadRequest("Dosya yüklenmedi.");
            }

            using (var httpClient = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Dosyayı okuyup form-data olarak ekleyelim
                    using (var stream = request.Image.OpenReadStream())
                    {
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                        formData.Add(fileContent, "image", request.Image.FileName);

                        // Flask API'ye isteği gönder
                        var response = await httpClient.PostAsync(flaskApiUrl, formData);

                        if (response.IsSuccessStatusCode)
                        {
                            // Yanıtı okuyalım
                            var responseContent = await response.Content.ReadAsStringAsync();
                            return Ok(responseContent);
                        }
                        else
                        {
                            return StatusCode((int)response.StatusCode, "Flask API'ye bağlanılamadı.");
                        }
                    }
                }
            }
        }
    }

    public class TumorDetectionRequest
    {
        public IFormFile Image { get; set; }
    }
}
