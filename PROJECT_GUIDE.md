# Understanding This Project

This is a companion to `README.md` (which covers the formal lab
deliverables). This file is meant to be read together with your partner —
it walks through *how the code actually works* and *why it's organized this
way*, in plain language, so you can both explain it if asked.

---

## 1. What this project actually does

One use case, start to finish: **a student tries to borrow a piece of
equipment, and the system either approves it or tells you exactly why not.**

That's it. Everything in this repo exists to support that one operation
cleanly. There's no UI, no database — just the decision-making logic and
enough scaffolding to prove it works.

---

## 2. The four projects, in the order data flows through them

Think of it as four concentric layers. Follow a borrow request through
them:

```
ConsoleDemo  →  Application  →  Domain
                     ↓
              Infrastructure
```

### Domain — "the nouns"
`src/EquipmentBorrowing.Domain/`

Plain C# classes with no idea that repositories, services, or a console app
even exist. Each one models a *thing* in the real scenario:

- **`Student.cs`** — id, name, whether they're allowed to borrow, and how
  many items they're allowed to have out at once.
- **`Equipment.cs`** — id, name, and whether it's available. It knows how to
  flip itself from available → borrowed (`MarkAsBorrowed()`) and back
  (`MarkAsReturned()`), and it refuses to be "borrowed" twice in a row.
- **`Borrowing.cs`** — one transaction record: which student, which
  equipment, when borrowed, when due, and its status. It knows how to mark
  itself returned (`MarkAsReturned()`), and it refuses to be returned twice.
- **`BorrowingStatus.cs`** — just the enum `Active` / `Returned`.

**Why this matters:** if you were asked "why is `MarkAsBorrowed()` on the
`Equipment` class instead of just setting `equipment.IsAvailable = false`
from outside?" — the answer is that the class protects its own rules. No
matter who calls it or from where, `Equipment` will never let itself be
marked as borrowed twice. If `IsAvailable` were a plain public setter,
*anyone* anywhere in the codebase could set it incorrectly and nothing
would stop them.

### Application — "the verbs"
`src/EquipmentBorrowing.Application/`

This is where the actual business decision happens.

- **`Interfaces/`** — three interfaces (`IStudentRepository`,
  `IEquipmentRepository`, `IBorrowingRepository`). These are *contracts*,
  not implementations. They say "whoever stores this data must be able to
  do these specific things" without saying how.
- **`Services/BorrowEquipmentService.cs`** — the star of the show. Its
  `ExecuteAsync` method is a straight-line checklist:
  1. Does the student exist?
  2. Is the student allowed to borrow?
  3. Does the equipment exist?
  4. Is the equipment available?
  5. Is the student already at their borrowing limit?
  6. If all of that passes → mark the equipment borrowed, create the
     borrowing record, done.
- **`Common/`** — two small helper types:
  - `BorrowEquipmentRequest` — the input (student id, equipment id, dates).
  - `BorrowEquipmentResult` — the output. Either "success, here's the
    borrowing" or "failure, here's *which* rule it broke"
    (`BorrowFailureReason`).

**Why this matters:** notice `BorrowEquipmentService` never says the word
"SQLite," "file," or "console" anywhere. It only talks to the three
interfaces. That's the whole point of Part D/E/F in the lab — the *decision
logic* doesn't care where the data lives.

### Infrastructure — "where the data actually lives (for now)"
`src/EquipmentBorrowing.Infrastructure/`

Three classes, one per interface, each backed by a plain C# `Dictionary` or
`List` instead of a real database:

- `InMemoryStudentRepository`
- `InMemoryEquipmentRepository`
- `InMemoryBorrowingRepository`

**Why this matters:** these are the *only* classes that would need to
change if you added SQLite later. You'd write `SqliteStudentRepository`,
`SqliteEquipmentRepository`, etc., have them implement the same three
interfaces, and swap them in — `BorrowEquipmentService` wouldn't need a
single line changed. That swap only works *because* Application never
references Infrastructure directly — Infrastructure references Application
(to implement its interfaces), not the other way around. If you get asked
about "dependency direction" in the defense, this is the concrete example
to point to.

### ConsoleDemo — "someone actually using it"
`src/EquipmentBorrowing.ConsoleDemo/Program.cs`

Plays the role a future Avalonia UI would eventually play: it creates the
three in-memory repositories, wires them into `BorrowEquipmentService`
through its constructor (this is the "dependency injection" from Part F —
nothing fancy, just passing objects in rather than `new`-ing them inside
the service), then fires off five requests and prints what happened.

---

## 3. Walking through one request end-to-end

Say `Program.cs` calls the service with `StudentId: 1, EquipmentId: 100`.

1. `BorrowEquipmentService.ExecuteAsync` asks `IStudentRepository` (really,
   the in-memory one) for student #1. Found — Juan Dela Cruz, allowed to
   borrow.
2. Asks `IEquipmentRepository` for equipment #100. Found — a multimeter,
   available.
3. Asks `IBorrowingRepository` how many *active* borrowings student #1
   currently has. Zero — under the limit.
4. All checks passed. Calls `equipment.MarkAsBorrowed()` — the `Equipment`
   object itself flips `IsAvailable` to `false` and would throw if it
   somehow was already unavailable.
5. Tells the equipment repository to persist that change
   (`UpdateAsync`) — in a real database this is where the `UPDATE`
   statement would fire.
6. Builds a new `Borrowing` object (with a placeholder id of `0`) and hands
   it to `IBorrowingRepository.AddAsync`. The in-memory repository assigns
   it a real id (like an auto-increment primary key would) and stores it.
7. Returns `BorrowEquipmentResult.Success(...)` with the persisted
   borrowing attached.
8. `Program.cs` sees `result.IsSuccess == true` and prints the approval
   line.

If any check in steps 1–3 had failed, the method would have returned
immediately with `BorrowEquipmentResult.Failure(...)` and a specific
`BorrowFailureReason` — nothing after that point would run.

---

## 4. Terms you might get asked to define

- **Repository pattern** — an interface that hides *how* data is stored
  behind a simple "get it" / "save it" API. Here: `IStudentRepository`,
  `IEquipmentRepository`, `IBorrowingRepository`.
- **Dependency injection (manual, not a container)** — instead of a class
  creating its own dependencies (`new EquipmentRepository()` inside the
  service), the dependencies are passed in through the constructor. This
  project does this in `BorrowEquipmentService`'s constructor and in
  `Program.cs` where it builds everything.
- **Separation of concerns** — each project has exactly one kind of
  responsibility (Domain = rules, Application = decisions, Infrastructure =
  storage), so a change to one doesn't force a change to the others.
- **Record** (`BorrowEquipmentRequest`) — a C# type built for holding
  immutable data with little to no behavior. Used here because a request is
  just a bundle of values, not something with its own logic.
- **Nullable reference types** (the `?` after types like `Student?`) — the
  compiler forces you to explicitly handle the case where
  `GetByIdAsync` didn't find anything, instead of letting a `null` sneak
  through unnoticed.

---

## 5. If you get asked "why didn't you just—"

- **"...put everything in one class?"** — Then a UI button click, a
  database query, and a business rule like "students can't exceed their
  borrowing limit" would all be tangled together. Changing the storage
  mechanism later would risk breaking the business rules, and vice versa.
- **"...let the service create its own repository?"** — Then you could
  never test `BorrowEquipmentService` without a real database, and you
  could never swap storage later without editing the service itself. See
  `BorrowEquipmentServiceTests.cs` — every test passes in-memory
  repositories instead.
- **"...skip the interfaces and just use the in-memory classes directly?"**
  — Works fine today, but the moment you add SQLite, you'd have to go
  rewrite `BorrowEquipmentService` to know about SQLite specifically. The
  interface is what lets that swap happen without touching the service.

---

## 6. What to actually do together before submitting

1. Both of you read `BorrowEquipmentService.ExecuteAsync` line by line out
   loud to each other — it's short, and it's the piece most likely to come
   up in questioning.
2. Run `dotnet run --project src/EquipmentBorrowing.ConsoleDemo` together
   and match each printed line back to which rule caused it.
3. Skim `BorrowEquipmentServiceTests.cs` — each test name describes exactly
   one rule from the checklist above.
4. Read the Reflection section in `README.md` — those are close to word-for-word
   the kind of questions you may get asked directly.
