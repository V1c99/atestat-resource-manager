using Microsoft.AspNetCore.Mvc;
using ResourceManager.Api.Contracts;
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
    public async Task<ActionResult<List<ResourceResponse>>> List([FromQuery] ResourceType? type, [FromQuery] bool includeInactive)
    {
        var resources = await _resources.ListAsync(type, !includeInactive);
        return Ok(resources.Select(r => r.ToResponse()).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResourceResponse>> Get(Guid id)
    {
        var resource = await _resources.GetAsync(id);
        if (resource is null)
        {
            return NotFound();
        }

        return Ok(resource.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<ResourceResponse>> Create(CreateResourceRequest request)
    {
        var created = await _resources.AddAsync(request.ToResource());
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResourceResponse>> Update(Guid id, UpdateResourceRequest request)
    {
        var updated = await _resources.UpdateAsync(id, request.Name, request.Type, request.Capacity, request.Location);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated.ToResponse());
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ResourceResponse>> Deactivate(Guid id)
    {
        var updated = await _resources.SetActiveAsync(id, false);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated.ToResponse());
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ResourceResponse>> Activate(Guid id)
    {
        var updated = await _resources.SetActiveAsync(id, true);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated.ToResponse());
    }
}
