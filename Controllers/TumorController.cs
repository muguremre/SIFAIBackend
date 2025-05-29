using Microsoft.AspNetCore.Mvc;
using SIFAIBackend.Business;
using SIFAIBackend.Entities;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SIFAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TumorController : ControllerBase
    {
        private readonly string flaskApiUrl = "http://127.0.0.1:5000/predict";
        private readonly ITumorService _tumorService;

        public TumorController(ITumorService tumorService)
        {
            _tumorService = tumorService;
        }

        [HttpPost("detect")]
        public async Task<IActionResult> DetectTumor([FromForm] TumorDetectionRequest request)
        {
            if (request.Image == null || request.Image.Length == 0 || request.UserId <= 0)
            {
                return BadRequest("Geçersiz talep. Lütfen tüm alanları doldurun.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            using (var httpClient = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            using (var imageStream = System.IO.File.OpenRead(filePath))
            {
                var fileContent = new StreamContent(imageStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                formData.Add(fileContent, "image", request.Image.FileName);
                var anamnezObj = new
                {
                    epilepsy = request.Epilepsy,
                    worsening_headache = request.WorseningHeadache,
                    morning_headache = request.MorningHeadache,
                    vision_loss = request.VisionLoss,
                    hormonal_issues = request.HormonalIssues,
                    family_history = request.FamilyHistory,
                    age = request.Age,
                    gender = request.Gender
                };

                string anamnezJson = JsonSerializer.Serialize(anamnezObj);
                formData.Add(new StringContent(anamnezJson), "anamnez_data");

                var response = await httpClient.PostAsync(flaskApiUrl, formData);
                if (response.IsSuccessStatusCode)
                {
                    var tumorTypeJson = await response.Content.ReadAsStringAsync();
                    var tumorTypeResponse = JsonSerializer.Deserialize<TumorTypeResponse>(tumorTypeJson);
                    var tumorType = tumorTypeResponse?.TumorType;

                    if (string.IsNullOrWhiteSpace(tumorType))
                        return BadRequest("Flask API geçersiz bir yanıt döndürdü.");

                    await _tumorService.SaveDetectionAsync(request.UserId, uniqueFileName, tumorType);

                    return Ok(tumorTypeResponse);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Flask API'ye bağlanılamadı.");
                }
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            if (userId <= 0)
                return BadRequest("Geçersiz kullanıcı ID'si.");

            var history = await _tumorService.GetHistoryByUserIdAsync(userId);

            if (history == null || history.Count == 0)
                return NotFound("Bu kullanıcı için geçmiş tespiti bulunamadı.");

            return Ok(history);
        }
    }

    public class TumorDetectionRequest
    {
        public int UserId { get; set; }
        public IFormFile Image { get; set; }

        public bool Epilepsy { get; set; }
        public bool WorseningHeadache { get; set; }
        public bool MorningHeadache { get; set; }
        public bool VisionLoss { get; set; }
        public bool HormonalIssues { get; set; }
        public bool FamilyHistory { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }

    public class TumorTypeResponse
    {
        [JsonPropertyName("tumor_type")]
        public string TumorType { get; set; }

        [JsonPropertyName("güven")]
        public string Confidence { get; set; }

        [JsonPropertyName("mr_tahmini")]
        public string MrTahmini { get; set; }

        [JsonPropertyName("mr_güven")]
        public string MrGüven { get; set; }

        [JsonPropertyName("anamnez_tahmini")]
        public string AnamnezTahmini { get; set; }

        [JsonPropertyName("anamnez_güven")]
        public string AnamnezGüven { get; set; }

        [JsonPropertyName("yorum")]
        public string Yorum { get; set; }
    }
}
