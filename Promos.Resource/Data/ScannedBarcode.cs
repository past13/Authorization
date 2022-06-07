namespace Promos.Resource.Data;

public class ScannedBarcode
{
    public long Id { get; set; }
    public string UserId { get; set; }
    public long OcrBarcodeMappingId { get; set; }
    public string Barcode { get; set; }
    public DateTime ScanDate { get; set; }
}
