namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a student who may be permitted to borrow laboratory equipment.
/// </summary>
/// <remarks>
/// This class owns only the facts and rules that belong to the student as a concept:
/// identity, name, whether the student is currently in good standing, and the
/// borrowing limit policy that applies to that student.
///
/// It deliberately does NOT own:
/// - the list of the student's borrowings (that belongs to the Borrowing repository,
///   since it is a query across many records, not a fact about the student itself);
/// - the decision of whether a specific borrowing request should be approved
///   (that is an application-level use case, since it requires coordinating the
///   Student, the Equipment, and the current Borrowing records together).
/// </remarks>
public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; private set; }
    public int MaxActiveBorrowings { get; }

    public Student(int id, string name, bool isAllowedToBorrow = true, int maxActiveBorrowings = 3)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name is required.", nameof(name));

        if (maxActiveBorrowings <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxActiveBorrowings), "A student must be allowed to borrow at least one item.");

        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
        MaxActiveBorrowings = maxActiveBorrowings;
    }

    /// <summary>
    /// Revokes the student's borrowing privilege (e.g. due to a policy violation).
    /// </summary>
    public void Suspend() => IsAllowedToBorrow = false;

    /// <summary>
    /// Restores the student's borrowing privilege.
    /// </summary>
    public void Reinstate() => IsAllowedToBorrow = true;
}
