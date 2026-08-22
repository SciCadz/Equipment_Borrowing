namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a single borrowing transaction linking one student to one piece
/// of equipment for a given period.
/// </summary>
/// <remarks>
/// Owns the facts of the transaction (who, what, when) and its own status
/// transition (Active -> Returned). It intentionally stores the student and
/// equipment as identifiers (StudentId / EquipmentId) rather than object
/// references, so this class does not need to know how those objects are
/// loaded or persisted.
///
/// It does NOT own:
/// - the eligibility checks that decide whether a borrowing should be created
///   in the first place (that belongs to the application service, since it
///   needs to inspect the Student, the Equipment, and other Borrowing records
///   together);
/// - how it is stored (that belongs to a repository implementation).
/// </remarks>
public class Borrowing
{
    public int Id { get; }
    public int StudentId { get; }
    public int EquipmentId { get; }
    public DateOnly DateBorrowed { get; }
    public DateOnly ExpectedReturnDate { get; }
    public DateOnly? ActualReturnDate { get; private set; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        int id,
        int studentId,
        int equipmentId,
        DateOnly dateBorrowed,
        DateOnly expectedReturnDate)
    {
        if (expectedReturnDate < dateBorrowed)
            throw new ArgumentException("Expected return date cannot precede the borrow date.", nameof(expectedReturnDate));

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    /// <summary>
    /// Marks the borrowing as returned. Throws if it has already been returned,
    /// so the same record cannot be "returned" twice.
    /// </summary>
    public void MarkAsReturned(DateOnly returnDate)
    {
        if (Status == BorrowingStatus.Returned)
            throw new InvalidOperationException($"Borrowing #{Id} has already been marked as returned.");

        ActualReturnDate = returnDate;
        Status = BorrowingStatus.Returned;
    }
}
