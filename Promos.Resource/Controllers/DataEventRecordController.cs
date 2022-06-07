using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promos.Resource.Data;
using Promos.Resource.Repositories;

namespace Promos.Resource.Controllers;

[Authorize("dataEventRecordsPolicy")]
[Route("api/[controller]")]
public class DataEventRecordsController : Controller
{
    private readonly DataEventRecordRepository _dataEventRecordRepository;

    public DataEventRecordsController(DataEventRecordRepository dataEventRecordRepository)
    {
        _dataEventRecordRepository = dataEventRecordRepository;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_dataEventRecordRepository.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        return Ok(_dataEventRecordRepository.Get(id));
    }

    [HttpPost]
    public void Post([FromBody]ScannedBarcode value)
    {
        _dataEventRecordRepository.Post(value);
    }

    [HttpPut("{id}")]
    public void Put(long id, [FromBody]ScannedBarcode value)
    {
        _dataEventRecordRepository.Put(id, value);
    }

    [HttpDelete("{id}")]
    public void Delete(long id)
    {
        _dataEventRecordRepository.Delete(id);
    }
}