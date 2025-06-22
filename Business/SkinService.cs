using Microsoft.EntityFrameworkCore;
using SIFAIBackend.DataAccess;

public class SkinService : ISkinService
{
    private readonly SifaiContext _context;

    public SkinService(SifaiContext context)
    {
        _context = context;
    }

    public async Task<int> SaveDetectionAsync(
        int userId,
        string imageUrl,
        double gorselSkor,
        double anamnezSkor,
        double ensembleSkor,
        string tahmin,
        string risk,
        string yorum,
        string prediction
        )
    {
        var rec = new SkinDetectionHistory
        {
            UserId = userId,
            DetectionDate = DateTime.UtcNow,
            ImageUrl = imageUrl,
            GorselSkor = gorselSkor,
            AnamnezSkor = anamnezSkor,
            EnsembleSkor = ensembleSkor,
            Tahmin = tahmin,
            Risk = risk,
            Yorum = yorum,
            Prediction = prediction
        };

        _context.SkinDetectionHistories.Add(rec);
        await _context.SaveChangesAsync();
        return rec.Id;
    }

    public async Task<List<SkinDetectionHistory>> GetHistoryByUserIdAsync(int userId)
    {
        return await _context.SkinDetectionHistories
            .Where(x => x.UserId == userId)
            .Select(x => new SkinDetectionHistory
            {
                Id = x.Id,
                UserId = x.UserId,
                ImageUrl = x.ImageUrl,
                Prediction = x.Prediction ?? "",
                Risk = x.Risk ?? "",
                DetectionDate = x.DetectionDate ?? DateTime.MinValue,
                GorselSkor = x.GorselSkor ?? 0,
                AnamnezSkor = x.AnamnezSkor ?? 0,
                EnsembleSkor = x.EnsembleSkor ?? 0,
                Tahmin = x.Tahmin ?? "",
                Yorum = x.Yorum ?? ""
            })
            .OrderByDescending(x => x.DetectionDate)
            .ToListAsync();
    }

}
