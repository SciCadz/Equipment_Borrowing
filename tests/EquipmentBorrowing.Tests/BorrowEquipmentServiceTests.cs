using EquipmentBorrowing.Application.Common;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    private static readonly DateOnly DateBorrowed = new(2026, 8, 28);
    private static readonly DateOnly ExpectedReturnDate = new(2026, 9, 4);

    private static BorrowEquipmentService CreateService(
        out InMemoryEquipmentRepository equipmentRepository,
        out InMemoryBorrowingRepository borrowingRepository,
        IEnumerable<Student>? students = null,
        IEnumerable<Equipment>? equipment = null)
    {
        var studentRepository = new InMemoryStudentRepository(
            students ?? new[] { new Student(1, "Juan Dela Cruz") });

        equipmentRepository = new InMemoryEquipmentRepository(
            equipment ?? new[] { new Equipment(100, "Digital Multimeter") });

        borrowingRepository = new InMemoryBorrowingRepository();

        return new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ApprovesBorrowing_WhenAllRulesAreSatisfied()
    {
        var service = CreateService(out var equipmentRepository, out _);

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 100, DateBorrowed, ExpectedReturnDate));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Borrowing);
        Assert.Equal(BorrowingStatus.Active, result.Borrowing!.Status);

        var storedEquipment = await equipmentRepository.GetByIdAsync(100);
        Assert.False(storedEquipment!.IsAvailable);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenStudentDoesNotExist()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(999, 100, DateBorrowed, ExpectedReturnDate));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.StudentNotFound, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenStudentIsNotAllowedToBorrow()
    {
        var service = CreateService(
            out _, out _,
            students: new[] { new Student(1, "Maria Santos", isAllowedToBorrow: false) });

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 100, DateBorrowed, ExpectedReturnDate));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.StudentNotAllowedToBorrow, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenEquipmentDoesNotExist()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 999, DateBorrowed, ExpectedReturnDate));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.EquipmentNotFound, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenEquipmentIsNotAvailable()
    {
        var service = CreateService(
            out _, out _,
            equipment: new[] { new Equipment(100, "Oscilloscope", isAvailable: false) });

        var result = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 100, DateBorrowed, ExpectedReturnDate));

        Assert.False(result.IsSuccess);
        Assert.Equal(BorrowFailureReason.EquipmentNotAvailable, result.FailureReason);
    }

    [Fact]
    public async Task ExecuteAsync_Fails_WhenStudentReachedTheActiveBorrowingLimit()
    {
        var service = CreateService(
            out _, out _,
            students: new[] { new Student(1, "Juan Dela Cruz", maxActiveBorrowings: 1) },
            equipment: new[]
            {
                new Equipment(100, "Digital Multimeter"),
                new Equipment(101, "Function Generator")
            });

        var first = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 100, DateBorrowed, ExpectedReturnDate));
        Assert.True(first.IsSuccess);

        var second = await service.ExecuteAsync(
            new BorrowEquipmentRequest(1, 101, DateBorrowed, ExpectedReturnDate));

        Assert.False(second.IsSuccess);
        Assert.Equal(BorrowFailureReason.BorrowingLimitReached, second.FailureReason);
    }
}
