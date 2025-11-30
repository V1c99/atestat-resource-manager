using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourceManager.Api.Contracts;
using ResourceManager.Infrastructure;

namespace ResourceManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> List()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        return Ok(users.Select(u => u.ToResponse()).ToList());
    }
}
