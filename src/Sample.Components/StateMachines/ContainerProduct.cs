namespace Sample.Components.StateMachines;

public class ContainerProduct :
    IEquatable<ContainerProduct>
{
    public string? Sku { get; set; }
    public string? SerialNumber { get; set; }
    public long OrderLine { get; set; }

    public bool Equals(ContainerProduct? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Sku == other.Sku && SerialNumber == other.SerialNumber && OrderLine == other.OrderLine;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((ContainerProduct)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            var hashCode = Sku != null ? Sku.GetHashCode() : 0;
            hashCode = (hashCode * 397) ^ (SerialNumber != null ? SerialNumber.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ OrderLine.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(ContainerProduct? left, ContainerProduct? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ContainerProduct? left, ContainerProduct? right)
    {
        return !Equals(left, right);
    }
}