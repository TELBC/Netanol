namespace Fennec.Utils;

public class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[] obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        unchecked
        {
            return obj.Aggregate(17, (current, b) => current * 31 + b);
        }
    }
}