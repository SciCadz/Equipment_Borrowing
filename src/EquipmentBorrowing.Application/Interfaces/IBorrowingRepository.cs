using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Borrowing records actually live. Only exposes
/// what the Borrow Equipment use case needs: counting a student's currently
/// active borrowings (to enforce the borrowing limit) and persisting a new
/// borrowing once it is approved.
/// </summary>
public interface IBorrowingRepository
{
    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new borrowing and returns the stored entity (with its
    /// assigned identity). The caller passes in a borrowing built with a
    /// placeholder id; the repository is responsible for assigning the real one.
    /// </summary>
    Task<Borrowing> AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}
