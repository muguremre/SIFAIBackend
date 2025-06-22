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
        string yorum)
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
            Yorum = yorum
        };

        _context.SkinDetectionHistories.Add(rec);
        await _context.SaveChangesAsync();
        return rec.Id;
    }

    public async Task<List<SkinDetectionHistory>> GetHistoryByUserIdAsync(int userId)
    {
        return await Task.FromResult(
            _context.SkinDetectionHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.DetectionDate)
                .ToList()
        );
    }
}
