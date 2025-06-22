public class SkinDetectionHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime DetectionDate { get; set; }
    public string ImageUrl { get; set; }

    public double GorselSkor { get; set; }
    public double AnamnezSkor { get; set; }
    public double EnsembleSkor { get; set; }

    public string Tahmin { get; set; }
    public string Risk { get; set; }
    public string Yorum { get; set; }
}
