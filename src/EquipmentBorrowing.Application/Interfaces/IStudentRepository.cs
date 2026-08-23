using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Student data actually lives (in-memory, a file,
/// a database, etc.). The application layer only needs to look a student up
/// by id, so that is the only method exposed here.
/// </summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
