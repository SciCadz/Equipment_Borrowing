using EquipmentBorrowing.Application.Common;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Executes the "Borrow Equipment" use case: checks every business rule from
/// the scenario, then creates and persists the borrowing if all rules pass.
///
/// This class coordinates domain objects and repository abstractions only.
/// It never creates a database connection, executes SQL, or references any
/// user-interface type — those concerns belong to Infrastructure and to
/// whatever presentation layer (console, Avalonia, etc.) calls this service.
/// </summary>
public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowEquipmentResult> ExecuteAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return BorrowEquipmentResult.Failure(BorrowFailureReason.StudentNotFound);

        if (!student.IsAllowedToBorrow)
            return BorrowEquipmentResult.Failure(BorrowFailureReason.StudentNotAllowedToBorrow);

        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return BorrowEquipmentResult.Failure(BorrowFailureReason.EquipmentNotFound);

        if (!equipment.IsAvailable)
            return BorrowEquipmentResult.Failure(BorrowFailureReason.EquipmentNotAvailable);

        var activeBorrowingCount = await _borrowingRepository.CountActiveByStudentIdAsync(student.Id, cancellationToken);
        if (activeBorrowingCount >= student.MaxActiveBorrowings)
            return BorrowEquipmentResult.Failure(BorrowFailureReason.BorrowingLimitReached);

        // All rules satisfied: perform the transaction.
        equipment.MarkAsBorrowed();
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        var newBorrowing = new Borrowing(
            id: 0, // placeholder; the repository assigns the real identity on insert
            studentId: student.Id,
            equipmentId: equipment.Id,
            dateBorrowed: request.DateBorrowed,
            expectedReturnDate: request.ExpectedReturnDate);

        var persistedBorrowing = await _borrowingRepository.AddAsync(newBorrowing, cancellationToken);

        return BorrowEquipmentResult.Success(persistedBorrowing);
    }
}
