using EquipmentBorrowing.Application.Common;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

// ---- Seed data ---------------------------------------------------------

var students = new List<Student>
{
    new(id: 1, name: "Juan Dela Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 1),
    new(id: 2, name: "Maria Santos", isAllowedToBorrow: false)
};

var equipment = new List<Equipment>
{
    new(id: 100, name: "Digital Multimeter"),
    new(id: 101, name: "Oscilloscope", isAvailable: false),
    new(id: 102, name: "Function Generator")
};

var studentRepository = new InMemoryStudentRepository(students);
var equipmentRepository = new InMemoryEquipmentRepository(equipment);
var borrowingRepository = new InMemoryBorrowingRepository();

var borrowEquipmentService = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository);

var today = DateOnly.FromDateTime(DateTime.Today);
var dueDate = today.AddDays(7);

// ---- Successful case -----------------------------------------------------

await RunScenarioAsync(
    "Successful borrow",
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 100, DateBorrowed: today, ExpectedReturnDate: dueDate));

// ---- Failure cases ---------------------------------------------------------

await RunScenarioAsync(
    "Failure - equipment does not exist",
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 999, DateBorrowed: today, ExpectedReturnDate: dueDate));

await RunScenarioAsync(
    "Failure - equipment is unavailable",
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 101, DateBorrowed: today, ExpectedReturnDate: dueDate));

await RunScenarioAsync(
    "Failure - student is not allowed to borrow",
    new BorrowEquipmentRequest(StudentId: 2, EquipmentId: 102, DateBorrowed: today, ExpectedReturnDate: dueDate));

await RunScenarioAsync(
    "Failure - student reached the active borrowing limit",
    new BorrowEquipmentRequest(StudentId: 1, EquipmentId: 102, DateBorrowed: today, ExpectedReturnDate: dueDate));

// ---- Local helper ---------------------------------------------------------

async Task RunScenarioAsync(string label, BorrowEquipmentRequest request)
{
    Console.WriteLine($"--- {label} ---");

    var result = await borrowEquipmentService.ExecuteAsync(request);

    if (result.IsSuccess)
    {
        Console.WriteLine(
            $"APPROVED: Borrowing #{result.Borrowing!.Id} — " +
            $"student {request.StudentId} borrowed equipment {request.EquipmentId}, " +
            $"due {result.Borrowing.ExpectedReturnDate:yyyy-MM-dd}.");
    }
    else
    {
        Console.WriteLine($"DENIED: {result.FailureReason}");
    }

    Console.WriteLine();
}
