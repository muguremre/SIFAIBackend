using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

[ApiController]
[Route("api/[controller]")]
public class SkinCancerController : ControllerBase
{
    private readonly ISkinService _skinService;
    private readonly string flaskSkinUrl = "http://127.0.0.1:5000/predict/skin";

    public SkinCancerController(ISkinService skinService)
    {
        _skinService = skinService;
    }

    [HttpPost("detect")]
    public async Task<IActionResult> Detect([FromForm] SkinCancerDetectionRequest request)
    {
        if (request.Image == null || request.Image.Length == 0 || request.UserId <= 0)
            return BadRequest("Lütfen geçerli bir resim ve UserId gönderin.");

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        var uniqueName = $"{Guid.NewGuid()}_{request.Image.FileName}";
        var path = Path.Combine(uploads, uniqueName);
        await using (var fs = new FileStream(path, FileMode.Create))
            await request.Image.CopyToAsync(fs);

        using var http = new HttpClient();
        using var formData = new MultipartFormDataContent();
        using var imageStream = System.IO.File.OpenRead(path);

        var imgContent = new StreamContent(imageStream);
        imgContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        formData.Add(imgContent, "image", request.Image.FileName);

        var anamnezJson = JsonSerializer.Serialize(request.Anamnez);
        formData.Add(new StringContent(anamnezJson), "anamnez_data");

        var response = await http.PostAsync(flaskSkinUrl, formData);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, "Flask API hatası");

        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SkinCancerResponse>(body);
        if (result == null)
            return BadRequest("Flask geçersiz JSON döndürdü.");

        double.TryParse(result.GorselSkor, out var gorsel);
        double.TryParse(result.AnamnezSkor, out var anamnez);
        double.TryParse(result.EnsembleSkor, out var ensemble);

        await _skinService.SaveDetectionAsync(
            request.UserId, uniqueName,
            gorsel, anamnez, ensemble,
            result.Tahmin, result.Risk, result.Yorum,
            result.Tahmin
        );

        return Ok(result);
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> History(int userId)
    {
        if (userId <= 0)
            return BadRequest("Geçersiz UserId");

        var history = await _skinService.GetHistoryByUserIdAsync(userId);
        if (history == null || !history.Any())
            return NotFound("Tespit geçmişi yok");

        return Ok(history);
    }
}
public class SkinCancerDetectionRequest
{
    public int UserId { get; set; }
    public IFormFile Image { get; set; }
    public SkinAnamnez Anamnez { get; set; }
}

public class SkinAnamnez
{
    public int fark_suresi { get; set; }
    public int renk_degisti { get; set; }
    public int kenar_duzensiz { get; set; }
    public int kasinti_kanama { get; set; }
    public int ailede_kanser { get; set; }
    public int gunes_maruz { get; set; }
    public int lezyon_kabuk { get; set; }
    public int travma_sonrasi { get; set; }
    public int ten_rengi { get; set; }
    public int tedavi_alindi { get; set; }
    public int bolge { get; set; }
}
public class SkinCancerResponse
{
    [JsonPropertyName("gorsel_skor")]
    public string GorselSkor { get; set; }

    [JsonPropertyName("anamnez_skor")]
    public string AnamnezSkor { get; set; }

    [JsonPropertyName("ensemble_skor")]
    public string EnsembleSkor { get; set; }

    [JsonPropertyName("tahmin")]
    public string Tahmin { get; set; }

    [JsonPropertyName("risk")]
    public string Risk { get; set; }

    [JsonPropertyName("yorum")]
    public string Yorum { get; set; }
}

