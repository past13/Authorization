using Microsoft.AspNetCore.Mvc;
using Promos.Resource.Data;

namespace Promos.Resource.Repositories;

public class DataEventRecordRepository
{
    private readonly ApplicationDbContext _context;

    public DataEventRecordRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ScannedBarcode> GetAll()
    {
        var data =  _context.ScannedBarcodes.ToList();

        return data;
    }

    public ScannedBarcode Get(long id)
    {
        var dataEventRecord = _context.ScannedBarcodes.First(t => t.Id == id);
        return dataEventRecord;
    }

    public void Post(ScannedBarcode dataEventRecord)
    {
        if(string.IsNullOrWhiteSpace(dataEventRecord.Barcode))
        {
            dataEventRecord.ScanDate = DateTime.UtcNow;
        };
        
        _context.ScannedBarcodes.Add(dataEventRecord);
        _context.SaveChanges();
    }

    public void Put(long id, [FromBody]ScannedBarcode dataEventRecord)
    {
        dataEventRecord.ScanDate = DateTime.UtcNow;
        _context.ScannedBarcodes.Update(dataEventRecord);
        _context.SaveChanges();
    }

    public void Delete(long id)
    {
        var entity = _context.ScannedBarcodes.First(t => t.Id == id);
        _context.ScannedBarcodes.Remove(entity);
        _context.SaveChanges();
    }
}