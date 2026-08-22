namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a single piece of borrowable laboratory equipment.
/// </summary>
/// <remarks>
/// Owns its identity, name, and availability, plus the two state transitions
/// that are intrinsic to "being a piece of equipment" (becoming borrowed,
/// becoming available again).
///
/// It does NOT own:
/// - who currently has it, or for how long (that is recorded on Borrowing,
///   since a piece of equipment does not need to know its own history to
///   answer "am I available right now?");
/// - the eligibility rules for who is allowed to borrow it (that is an
///   application-level concern that also depends on the Student).
/// </remarks>
public class Equipment
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }

    /// <summary>
    /// Marks the equipment as currently borrowed. Throws if it is already unavailable,
    /// so an invalid state transition cannot be forced from outside the class.
    /// </summary>
    public void MarkAsBorrowed()
    {
        if (!IsAvailable)
            throw new InvalidOperationException($"Equipment '{Name}' (Id={Id}) is not available to borrow.");

        IsAvailable = false;
    }

    /// <summary>
    /// Marks the equipment as available again after it has been returned.
    /// </summary>
    public void MarkAsReturned() => IsAvailable = true;
}
