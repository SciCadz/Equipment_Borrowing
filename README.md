# Campus Equipment Borrowing System

Laboratory Activity 1 — From Requirements to Application Structure.

This repository contains the architectural foundation for the system (Domain,
Application, Infrastructure, Tests) with one fully implemented use case,
**Borrow Equipment**, demonstrated both by a console program and by an
automated test suite. No database and no graphical interface are included,
per the activity's scope.

---

## Part A — Analysis

### A. Actors

| Actor | What they expect from the system |
|---|---|
| **Student** | To be able to request equipment and immediately know whether the request is approved or denied, and why. |
| **Laboratory Staff / Custodian** (implied) | To trust that the system enforces borrowing rules correctly, so equipment inventory and student borrowing history stay accurate without manual checking. |

The scenario describes only one active initiator of requests (the Student);
laboratory staff are an implied actor because someone is ultimately
responsible for the equipment records the system protects, even though the
scenario does not describe a staff-facing action.

### B. Use Cases

| Item | Description |
|---|---|
| Use Case | **Borrow Equipment** |
| Primary Actor | Student |
| Preconditions | Student is registered and currently allowed to borrow; equipment catalog exists. |
| Main Action | Student requests to borrow a specific, available piece of equipment. |
| Expected Result | A new borrowing record is created with status Active; the equipment becomes unavailable. |
| Possible Failure | Student not allowed to borrow, equipment not found, equipment unavailable, or student already at the maximum number of active borrowings. |

| Item | Description |
|---|---|
| Use Case | **Return Equipment** |
| Primary Actor | Student |
| Preconditions | An Active borrowing record exists linking the student to the equipment. |
| Main Action | Student returns the borrowed equipment. |
| Expected Result | The borrowing is marked Returned; the equipment becomes available again. |
| Possible Failure | No matching active borrowing exists for that student/equipment pair (e.g. already returned). |

| Item | Description |
|---|---|
| Use Case | **Find Available Equipment** |
| Primary Actor | Student |
| Preconditions | Equipment catalog exists. |
| Main Action | Student browses or searches for equipment that is currently available. |
| Expected Result | A list of equipment currently marked available is returned. |
| Possible Failure | No equipment currently matches the availability filter (an empty result, not necessarily an error). |

> Only **Borrow Equipment** is implemented in this activity, as instructed in
> Part E. Return Equipment and Find Available Equipment are documented here
> because they were used in Part A/D to reason about what the domain models
> and repositories need to support later, but their application services and
> supporting repository methods are intentionally left out of this
> submission so that no interface method exists without a use case that
> currently needs it.

### C. Domain Concepts

**Student**
1. Must contain: identity, name, whether currently allowed to borrow, and the maximum number of active borrowings permitted.
2. Rules/state it owns: its own eligibility flag; the borrowing-limit *policy* value.
3. Not its responsibility: tracking which specific items it currently has borrowed (that is a fact about `Borrowing` records, queried when needed, not duplicated state that could go stale), or deciding whether a particular request should be approved (that requires looking at `Equipment` too, which is beyond what a `Student` alone can know).

**Equipment**
1. Must contain: identity, name, and current availability.
2. Rules/state it owns: the transition between available and borrowed (it can validate that it isn't already borrowed before allowing that transition).
3. Not its responsibility: knowing who currently has it or for how long (that belongs to `Borrowing`), or deciding eligibility (that depends on the `Student`).

**Borrowing**
1. Must contain: which student, which equipment, the date borrowed, the expected return date, the actual return date (once returned), and its status.
2. Rules/state it owns: the Active → Returned transition, and the invariant that the expected return date cannot precede the borrow date.
3. Not its responsibility: deciding whether it should have been created in the first place (that is the application service's job, since it needs the `Student`, the `Equipment`, and the count of other `Borrowing` records together).

---

## Part I — Architecture Explanation

### 1. Solution Structure

- **Domain** — the concepts and rules that exist regardless of how the
  system is built or deployed: `Student`, `Equipment`, `Borrowing`,
  `BorrowingStatus`. No dependency on any other project.
- **Application** — the use cases the system performs, expressed as
  services (`BorrowEquipmentService`) that coordinate Domain objects through
  repository *interfaces* (`IStudentRepository`, `IEquipmentRepository`,
  `IBorrowingRepository`), plus the request/result types those services use.
  Depends only on Domain.
- **Infrastructure** — concrete, swappable implementations of the
  Application-layer interfaces. For this activity, three in-memory
  repositories (`InMemoryStudentRepository`, `InMemoryEquipmentRepository`,
  `InMemoryBorrowingRepository`). Depends on Domain and Application.
- **Tests** — automated tests for `BorrowEquipmentService`, covering the
  successful path and every failure rule. Depends on all three projects
  above, the same way a future UI would.

A separate `EquipmentBorrowing.ConsoleDemo` project is included under `src/`
as the "minimal executable program" required by Part H — it wires the
in-memory repositories to the service and prints one successful and several
unsuccessful borrowing attempts to the console.

### 2. Dependency Direction

```text
ConsoleDemo / Tests / Future Avalonia UI
          │
          ▼
     Application  ───uses interfaces implemented by───▶  (implemented in Infrastructure)
       │      ▲
       ▼      │
     Domain   │
              │
     Infrastructure
```

Application depends on Domain, and defines the repository *interfaces*.
Infrastructure depends on both Domain and Application, because it must
implement Application's interfaces using Domain's types — but Application
never references Infrastructure. This is what allows the storage mechanism
to change later without touching the business logic: Application only knows
about `IEquipmentRepository`, never `InMemoryEquipmentRepository` or any
future `SqliteEquipmentRepository`.

### 3. Use Case Mapping

```text
Actor:                          Student
Use Case:                       Borrow Equipment
Application Service:            BorrowEquipmentService
Domain Objects Used:            Student, Equipment, Borrowing, BorrowingStatus
Repository Interfaces Used:     IStudentRepository, IEquipmentRepository, IBorrowingRepository
Infrastructure Implementations: InMemoryStudentRepository, InMemoryEquipmentRepository, InMemoryBorrowingRepository
```

### 4. Reflection

**1. Why should the application service depend on a repository interface
instead of directly depending on a database implementation?**
Because the business rules for borrowing equipment (is the student allowed,
is the equipment available, has the limit been reached) have nothing to do
with *how* that data is stored. Depending on an interface means
`BorrowEquipmentService` can be tested with an in-memory fake today and
handed a SQLite-backed implementation later without a single line of the
service itself changing.

**2. Which parts of your current solution could remain unchanged if SQLite
were added later?**
All of Domain and Application. Only Infrastructure would change — the three
`InMemory...Repository` classes would be replaced (or supplemented) by
implementations that talk to SQLite, still satisfying the same three
interfaces.

**3. Which project would eventually contain Avalonia Views?**
A new UI project (e.g. `EquipmentBorrowing.Desktop`) sitting alongside
`ConsoleDemo`, at the same "outer" layer — it would reference Application
(to call `BorrowEquipmentService`) and Domain (to display the data returned),
but Infrastructure would only need to be referenced at startup to wire up
concrete repositories via dependency injection.

**4. Should an Avalonia button directly execute database queries? Why or
why not?**
No. If a button's click handler executed SQL directly, the business rules
(eligibility, availability, borrowing limits) would either have to live in
the UI code or be duplicated everywhere a similar action is triggered. Routing
the click handler through `BorrowEquipmentService` instead keeps the rules in
one place, keeps the UI free of persistence concerns, and keeps the logic
testable without launching a UI at all.

**5. What part of your implementation represents the actual business
operation requested by the actor?**
`BorrowEquipmentService.ExecuteAsync`. It is the one piece of code that
represents "a student borrowing equipment" as a whole — everything else
(the domain models, the repository interfaces, the in-memory
implementations) exists to support that single operation.

---

## Running the Demonstration

This environment did not have the .NET SDK available to compile and run the
project, so the build has not been verified end-to-end here. With the .NET 8
SDK installed:

```bash
dotnet build
dotnet run --project src/EquipmentBorrowing.ConsoleDemo
dotnet test
```

`dotnet run` prints one approved borrowing followed by four denied attempts
(equipment not found, equipment unavailable, student not allowed to borrow,
and student at the borrowing limit). `dotnet test` runs the six unit tests in
`tests/EquipmentBorrowing.Tests`, covering the same success case and every
failure rule individually.

Please run `dotnet build` yourself, take the required screenshot of a
successful build, and confirm `dotnet test` passes before submitting —
neither has been verified in the environment this was written in.
