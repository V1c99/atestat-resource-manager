using Microsoft.EntityFrameworkCore;
using ResourceManager.Domain;

namespace ResourceManager.Infrastructure;

public static class Seed
{
    public static async Task RunAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        db.Users.AddRange(
            new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), FullName = "Andrei Popescu", Email = "andrei.popescu@example.com", Role = UserRole.Administrator },
            new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), FullName = "Maria Ionescu", Email = "maria.ionescu@example.com", Role = UserRole.Staff },
            new User { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), FullName = "Elena Dumitru", Email = "elena.dumitru@example.com", Role = UserRole.Staff },
            new User { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), FullName = "Radu Marin", Email = "radu.marin@example.com", Role = UserRole.Staff });

        db.Resources.AddRange(
            new Resource { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), Name = "Boardroom", Type = ResourceType.MeetingRoom, Capacity = 14, Location = "Floor 3" },
            new Resource { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), Name = "Training room", Type = ResourceType.MeetingRoom, Capacity = 24, Location = "Floor 1" },
            new Resource { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), Name = "Interview room", Type = ResourceType.MeetingRoom, Capacity = 4, Location = "Floor 2" },
            new Resource { Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), Name = "Van B-114-RMG", Type = ResourceType.Vehicle, Capacity = 3, Location = "Car park" },
            new Resource { Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), Name = "Van B-227-RMG", Type = ResourceType.Vehicle, Capacity = 3, Location = "Car park" },
            new Resource { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Name = "Projector Epson EB-2247U", Type = ResourceType.Equipment, Capacity = 1, Location = "Store room" },
            new Resource { Id = Guid.Parse("cccccccc-0000-0000-0000-000000000002"), Name = "Camera kit", Type = ResourceType.Equipment, Capacity = 1, Location = "Store room" },
            new Resource { Id = Guid.Parse("dddddddd-0000-0000-0000-000000000001"), Name = "Test bench", Type = ResourceType.LabSlot, Capacity = 2, Location = "Workshop" });

        await db.SaveChangesAsync();
    }
}
