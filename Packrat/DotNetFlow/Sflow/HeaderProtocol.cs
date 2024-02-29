namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents the first protocol of the sampled packet inside the <see cref="RawPacketHeader"/>.
    /// </summary>
    public enum HeaderProtocol
    {
        Ethernet = 1,
        Ppp = 7,
        Ipv4 = 11,
        Ipv6 = 12,
        Mpls = 13,
    }
}