using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain;

namespace ResourceManager.Infrastructure.Repositories;

public class ResourceRepository
{
    private readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Resource>> ListAsync(ResourceType? type, bool onlyActive)
    {
        var query = _db.Resources.AsQueryable();

        if (type is not null)
        {
            query = query.Where(r => r.Type == type);
        }

        if (onlyActive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Resource?> GetAsync(Guid id)
    {
        return await _db.Resources.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Resource> AddAsync(Resource resource)
    {
        resource.Id = Guid.NewGuid();
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        return resource;
    }

    public async Task<Resource?> UpdateAsync(Guid id, string name, ResourceType type, int capacity, string location)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id);
        if (resource is null)
        {
            return null;
        }

        resource.Name = name;
        resource.Type = type;
        resource.Capacity = capacity;
        resource.Location = location;
        await _db.SaveChangesAsync();
        return resource;
    }

    public async Task<Resource?> SetActiveAsync(Guid id, bool isActive)
    {
        var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Id == id);
        if (resource is null)
        {
            return null;
        }

        resource.IsActive = isActive;
        await _db.SaveChangesAsync();
        return resource;
    }
}
