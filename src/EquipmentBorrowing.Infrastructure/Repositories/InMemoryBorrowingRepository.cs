using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Simple in-memory implementation of <see cref="IBorrowingRepository"/>.
/// Assigns each new borrowing a sequential id, the way an auto-increment
/// primary key would in a real database.
/// </summary>
public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();
    private int _nextId = 1;

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<Borrowing> AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        var persisted = new Borrowing(
            _nextId++,
            borrowing.StudentId,
            borrowing.EquipmentId,
            borrowing.DateBorrowed,
            borrowing.ExpectedReturnDate);

        _borrowings.Add(persisted);
        return Task.FromResult(persisted);
    }
}
