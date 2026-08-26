using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Simple in-memory implementation of <see cref="IStudentRepository"/>.
/// Stands in for a real data store (SQLite, a file, an API, etc.) so the
/// application layer can be exercised without any external dependency.
/// </summary>
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? seed = null)
    {
        _students = (seed ?? Enumerable.Empty<Student>()).ToDictionary(s => s.Id);
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _students.TryGetValue(id, out var student);
        return Task.FromResult(student);
    }
}
