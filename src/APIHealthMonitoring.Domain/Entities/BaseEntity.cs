namespace APIHealthMonitoring.Domain.Entities;

/// <summary>
/// Serves as the base class for all domain entities in the system.
/// Provides a common primary key and audit trail properties.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// The unique identifier for the entity.
    /// EF Core will map this to an INT IDENTITY(1,1) primary key column by convention.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The UTC timestamp of when this entity was first created.
    /// Should be set once on insertion and never updated.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The UTC timestamp of when this entity was last modified.
    /// Nullable because a record may never have been updated after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}