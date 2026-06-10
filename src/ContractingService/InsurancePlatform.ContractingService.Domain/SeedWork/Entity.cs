namespace InsurancePlatform.ContractingService.Domain.SeedWork;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (EqualityComparer<TId>.Default.Equals(Id, default) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default))
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
