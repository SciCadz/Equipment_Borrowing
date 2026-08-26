using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Simple in-memory implementation of <see cref="IEquipmentRepository"/>.
/// </summary>
public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly Dictionary<int, Equipment> _equipment;

    public InMemoryEquipmentRepository(IEnumerable<Equipment>? seed = null)
    {
        _equipment = (seed ?? Enumerable.Empty<Equipment>()).ToDictionary(e => e.Id);
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _equipment.TryGetValue(id, out var equipment);
        return Task.FromResult(equipment);
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        // With an in-memory dictionary of reference types, the entry is
        // already updated in place. A real store (SQLite, a file, an API)
        // would use this method to write the change through; the method is
        // kept here so the application layer never needs to know the
        // difference.
        _equipment[equipment.Id] = equipment;
        return Task.CompletedTask;
    }
}
