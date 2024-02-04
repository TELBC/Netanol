namespace Fennec.Utils;

public class IpPairComparer : IEqualityComparer<(byte[], byte[])>
{
    public bool Equals((byte[], byte[]) x, (byte[], byte[]) y)
    {
        return x.Item1.SequenceEqual(y.Item1) && x.Item2.SequenceEqual(y.Item2);
    }

    public int GetHashCode((byte[], byte[]) obj)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + GetSequenceHashCode(obj.Item1);
            hash = hash * 23 + GetSequenceHashCode(obj.Item2);
            return hash;
        }
    }

    private int GetSequenceHashCode(IEnumerable<byte> sequence)
    {
        unchecked
        {
            return sequence.Aggregate(19, (current, element) => current * 31 + element);
        }
    }
}