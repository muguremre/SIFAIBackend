public interface ISkinService
{
    Task<int> SaveDetectionAsync(
        int userId,
        string imageUrl,
        double gorselSkor,
        double anamnezSkor,
        double ensembleSkor,
        string tahmin,
        string risk,
        string yorum,
        string prediction
    );

    Task<List<SkinDetectionHistory>> GetHistoryByUserIdAsync(int userId);
}
