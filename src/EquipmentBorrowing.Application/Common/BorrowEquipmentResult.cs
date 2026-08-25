using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Common;

/// <summary>
/// The specific rule that rejected a borrowing request. "None" is used only
/// for a successful result and should never be inspected on its own.
/// </summary>
public enum BorrowFailureReason
{
    None,
    StudentNotFound,
    StudentNotAllowedToBorrow,
    EquipmentNotFound,
    EquipmentNotAvailable,
    BorrowingLimitReached
}

/// <summary>
/// Outcome of a Borrow Equipment attempt. Using a result object instead of
/// throwing exceptions for business-rule failures keeps "the equipment is
/// unavailable" (an expected, everyday outcome) distinct from an actual
/// error (e.g. the repository failing to connect).
/// </summary>
public class BorrowEquipmentResult
{
    public bool IsSuccess { get; }
    public BorrowFailureReason FailureReason { get; }
    public Borrowing? Borrowing { get; }

    private BorrowEquipmentResult(bool isSuccess, BorrowFailureReason failureReason, Borrowing? borrowing)
    {
        IsSuccess = isSuccess;
        FailureReason = failureReason;
        Borrowing = borrowing;
    }

    public static BorrowEquipmentResult Success(Borrowing borrowing) =>
        new(true, BorrowFailureReason.None, borrowing);

    public static BorrowEquipmentResult Failure(BorrowFailureReason reason) =>
        new(false, reason, null);
}
