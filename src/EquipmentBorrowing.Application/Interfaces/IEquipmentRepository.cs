using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Equipment data actually lives. Only exposes what
/// the Borrow Equipment use case needs: looking equipment up by id, and
/// persisting the availability change once a borrowing is approved.
/// </summary>
public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}
