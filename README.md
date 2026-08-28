# Campus Equipment Borrowing System

**ITSD 81 – Desktop Application Development | Laboratory Activity 1**

A small C#/.NET 8 solution for the **Campus Equipment Borrowing System**.  
This submission implements the **Borrow Equipment** use case using separated Domain, Application, Infrastructure, Console Demo, and Tests projects.

No database, Entity Framework Core, or Avalonia UI is used, as required for this activity.

---

## 1. Requirements and Use Cases

### Actor

**Student** — requests equipment and receives an approval or a reason for denial.

### Major Use Cases

| Use Case | Primary Actor | Expected Result | Possible Failure |
|---|---|---|---|
| Borrow Equipment | Student | Borrowing is created and equipment becomes unavailable | Student not found/not allowed, equipment not found/unavailable, borrowing limit reached |
| Return Equipment | Student | Borrowing becomes Returned and equipment becomes available | No matching active borrowing |
| Find Available Equipment | Student | Available equipment is listed | No matching equipment |

Only **Borrow Equipment** is implemented for this laboratory activity.

---

## 2. Solution Structure

```text
EquipmentBorrowing/
├── README.md
├── EquipmentBorrowing.sln
├── src/
│   ├── EquipmentBorrowing.Domain/
│   │   ├── Student.cs
│   │   ├── Equipment.cs
│   │   ├── Borrowing.cs
│   │   └── BorrowingStatus.cs
│   │
│   ├── EquipmentBorrowing.Application/
│   │   ├── Interfaces/
│   │   │   ├── IStudentRepository.cs
│   │   │   ├── IEquipmentRepository.cs
│   │   │   └── IBorrowingRepository.cs
│   │   ├── Common/
│   │   │   ├── BorrowEquipmentRequest.cs
│   │   │   └── BorrowEquipmentResult.cs
│   │   └── Services/
│   │       └── BorrowEquipmentService.cs
│   │
│   ├── EquipmentBorrowing.Infrastructure/
│   │   └── Repositories/
│   │       ├── InMemoryStudentRepository.cs
│   │       ├── InMemoryEquipmentRepository.cs
│   │       └── InMemoryBorrowingRepository.cs
│   │
│   └── EquipmentBorrowing.ConsoleDemo/
│       └── Program.cs
│
└── tests/
    └── EquipmentBorrowing.Tests/
        └── BorrowEquipmentServiceTests.cs
```

### Responsibilities

- **Domain** — `Student`, `Equipment`, `Borrowing`, and `BorrowingStatus`; contains domain state and state-transition rules.
- **Application** — repository interfaces, request/result types, and `BorrowEquipmentService`; contains the borrowing use-case logic.
- **Infrastructure** — in-memory implementations of the repository interfaces.
- **ConsoleDemo** — minimal executable demonstration of successful and failed borrowing requests.
- **Tests** — automated tests for the borrowing service.

---

## 3. Dependency Direction

```text
ConsoleDemo / Future UI / Tests
              │
              ▼
        Application
          │     ▲
          ▼     │
        Domain  │
                │
        Infrastructure
```

`Application` depends only on `Domain`.  
`Infrastructure` depends on `Application` and `Domain` because it implements the repository interfaces.  
The application service never creates a database connection or directly depends on a storage implementation.

This makes it possible to replace the in-memory repositories with SQLite repositories later without changing the borrowing business logic.

---

## 4. Implemented Use Case

```text
Actor:                          Student
Use Case:                       Borrow Equipment
Application Service:            BorrowEquipmentService
Domain Objects Used:            Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used:     IStudentRepository
                                IEquipmentRepository
                                IBorrowingRepository
Infrastructure Used:            InMemoryStudentRepository
                                InMemoryEquipmentRepository
                                InMemoryBorrowingRepository
```

The service checks:

1. Student exists.
2. Student is allowed to borrow.
3. Equipment exists.
4. Equipment is available.
5. Student has not reached the active borrowing limit.
6. If all checks pass, a borrowing is created and the equipment becomes unavailable.

Dependencies are supplied through the `BorrowEquipmentService` constructor (manual dependency injection).

---

## 5. Demonstration

The console program demonstrates:

- one successful borrowing;
- equipment not found;
- equipment unavailable;
- student not allowed to borrow;
- borrowing limit reached.

Run:

```bash
dotnet build
dotnet run --project src/EquipmentBorrowing.ConsoleDemo
```

The successful case prints an `APPROVED` message. Failure cases print `DENIED` with the corresponding reason.

---

## 6. Tests

The test project contains six tests covering:

- successful borrowing;
- student not found;
- student not allowed to borrow;
- equipment not found;
- equipment unavailable;
- borrowing limit reached.

Run:

```bash
dotnet test
```

---

## 7. Reflection

### 1. Why use repository interfaces?

The application service should depend on what the application needs, not on a particular storage technology. Interfaces allow the same business logic to work with in-memory storage now and SQLite later.

### 2. What can remain unchanged if SQLite is added?

The Domain models and `BorrowEquipmentService` can remain unchanged. New SQLite repository implementations can satisfy the existing interfaces.

### 3. Where would Avalonia Views go later?

A separate desktop/UI project, alongside the ConsoleDemo project.

### 4. Should an Avalonia button execute database queries directly?

No. The UI should call the application service. Database access belongs behind repository abstractions so business rules remain separate and testable.

### 5. What represents the actual business operation?

`BorrowEquipmentService.ExecuteAsync` represents the **Borrow Equipment** operation because it coordinates the validation rules, domain objects, and repositories needed to approve or reject the request.

---

## 8. Submission Checklist

The repository is intended to be submission-ready:

- [x] C#/.NET solution
- [x] Domain models
- [x] Repository abstractions
- [x] Application service
- [x] Manual dependency injection
- [x] In-memory repository implementations
- [x] Successful use-case demonstration
- [x] Failure-case demonstrations
- [x] Automated tests
- [x] Architecture explanation
- [x] Git history with meaningful development commits

Before submission, run:

```bash
dotnet build
dotnet test
dotnet run --project src/EquipmentBorrowing.ConsoleDemo
```

For the required screenshot, capture the terminal showing the **successful `dotnet build`** result. Then upload/push the repository to Git.
