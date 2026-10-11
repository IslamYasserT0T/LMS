using Microsoft.AspNetCore.Mvc;
using SampleModule.Application.Contracts;
using SampleModule.Application.DTOs.Sample;

namespace SampleModule.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SampleController : ControllerBase
{
    private readonly ISampleAppService _sampleAppService;
    public SampleController(ISampleAppService sampleAppService)
    {
        _sampleAppService = sampleAppService;
    }
    // GET: api/<SampleController>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _sampleAppService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _sampleAppService.GetByIdAsync(id);

        return Ok(result);
    }

    // POST api/<SampleController>
    [HttpPost]
    public async Task<IActionResult> Create( CreateUpdateSampleDto createUpdateSample)
    {
        var reuslt =  await _sampleAppService.CreateAsync(createUpdateSample);

        return Ok(reuslt);

    }

    // PUT api/<SampleController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUpdateSampleDto createUpdateSample)
    {
        var result =  await _sampleAppService.UpdateAsync(createUpdateSample, id );

        return Ok(result);

    }

    // DELETE api/<SampleController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result =  await _sampleAppService.DeleteAsync(id);

        return Ok(result);
    }
}
