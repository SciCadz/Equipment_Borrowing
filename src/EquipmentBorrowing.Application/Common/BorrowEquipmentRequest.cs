namespace EquipmentBorrowing.Application.Common;

/// <summary>
/// The input needed to attempt a borrowing. A record is a natural fit here:
/// it is an immutable bundle of data with no behavior of its own.
/// </summary>
public record BorrowEquipmentRequest(
    int StudentId,
    int EquipmentId,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate);
