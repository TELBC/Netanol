using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
///     Provides functionality to match IP addresses with a given mask to other IP addresses.
/// </summary>
public class IpAddressMatcher
{
    public IpAddressMatcher(byte[] address, byte[] mask, bool include)
    {
        Address = address;
        Mask = mask;
        Include = include;
    }

#pragma warning disable CS8618
    protected IpAddressMatcher()
    {
    }
#pragma warning restore CS8618

    /// <summary>
    ///     The address to match against.
    /// </summary>
    [BsonElement("address")]
    public byte[] Address { get; set; }

    /// <summary>
    ///     The mask to apply to the address and other address.
    /// </summary>
    [BsonElement("mask")]
    public byte[] Mask { get; set; }

    /// <summary>
    ///     Whether to include the address in the set. This is not used by the matcher itself, but by the layer that uses it.
    /// </summary>
    [BsonElement("include")]
    public bool Include { get; set; }

    /// <summary>
    ///     The address with the mask applied.
    /// </summary>
    [BsonIgnore]
    public byte[] MaskedAddress => Address.Select((b, i) => (byte)(b & Mask[i])).ToArray();

    /// <summary>
    ///     Returns whether the given address matches the address and mask of this matcher.
    /// </summary>
    public bool Match(byte[] address)
    {
        if (address.Length != Address.Length || address.Length != Mask.Length)
            return false;

        for (var i = 0; i < 4; i++)
            if ((address[i] & Mask[i]) != (Address[i] & Mask[i]))
                return false;

        return true;
    }
}

public record IpAddressMatcherDto(string Address, string Mask, bool Include);