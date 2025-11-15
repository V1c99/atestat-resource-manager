using Microsoft.AspNetCore.Mvc;
using ResourceManager.Domain;
using ResourceManager.Infrastructure.Repositories;

namespace ResourceManager.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly ResourceRepository _resources;

    public ResourcesController(ResourceRepository resources)
    {
        _resources = resources;
    }

    [HttpGet]
    public async Task<ActionResult<List<Resource>>> List([FromQuery] ResourceType? type, [FromQuery] bool includeInactive)
    {
        return Ok(await _resources.ListAsync(type, !includeInactive));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Resource>> Get(Guid id)
    {
        var resource = await _resources.GetAsync(id);
        if (resource is null)
        {
            return NotFound();
        }

        return Ok(resource);
    }

    [HttpPost]
    public async Task<ActionResult<Resource>> Create(Resource resource)
    {
        var created = await _resources.AddAsync(resource);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Resource>> Update(Guid id, Resource resource)
    {
        var updated = await _resources.UpdateAsync(id, resource.Name, resource.Type, resource.Capacity, resource.Location);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<Resource>> Deactivate(Guid id)
    {
        var updated = await _resources.SetActiveAsync(id, false);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<Resource>> Activate(Guid id)
    {
        var updated = await _resources.SetActiveAsync(id, true);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }
}
