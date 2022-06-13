using Microsoft.AspNetCore.Mvc;
using Promos.Resource.Data;

namespace Promos.Resource.Repositories;

public class ExternalApiRepository
{
    private readonly ApplicationDbContext _context;

    public ExternalApiRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ScannedBarcode> GetAll()
    {
        return _context.ScannedBarcodes.ToList();
    }

    public ScannedBarcode Get(long id)
    {
        return _context.ScannedBarcodes.First(sb => sb.Id == id);
    }

    public void Post(ScannedBarcode scannedBarcode)
    {
        if(string.IsNullOrWhiteSpace(scannedBarcode.Barcode))
        {
            scannedBarcode.ScanDate = DateTime.UtcNow;
        };
        
        _context.ScannedBarcodes.Add(scannedBarcode);
        _context.SaveChanges();
    }

    public void Put(long id, [FromBody]ScannedBarcode scannedBarcode)
    {
        scannedBarcode.ScanDate = DateTime.UtcNow;
        _context.ScannedBarcodes.Update(scannedBarcode);
        _context.SaveChanges();
    }

    public void Delete(long id)
    {
        var entity = _context.ScannedBarcodes.First(sb => sb.Id == id);
        _context.ScannedBarcodes.Remove(entity);
        _context.SaveChanges();
    }
}