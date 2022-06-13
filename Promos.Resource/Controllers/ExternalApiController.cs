using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Promos.Resource.Data;
using Promos.Resource.Repositories;

namespace Promos.Resource.Controllers;

[Authorize("externalApiPolicy")]
[Route("api/[controller]")]
public class ExternalApiController : Controller
{
    private readonly ExternalApiRepository _externalApiRepository;

    public ExternalApiController(ExternalApiRepository externalApiRepository)
    {
        _externalApiRepository = externalApiRepository;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_externalApiRepository.GetAll());
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        return Ok(_externalApiRepository.Get(id));
    }

    [HttpPost]
    public void Post([FromBody]ScannedBarcode value)
    {
        _externalApiRepository.Post(value);
    }

    [HttpPut("{id}")]
    public void Put(long id, [FromBody]ScannedBarcode value)
    {
        _externalApiRepository.Put(id, value);
    }

    [HttpDelete("{id}")]
    public void Delete(long id)
    {
        _externalApiRepository.Delete(id);
    }
}