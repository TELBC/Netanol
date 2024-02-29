namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents the interface information. Either input or output.
    /// According to the sFlow v5 specification, the input interface can only be <see cref="InterfaceFormat.SingleInterface"/>.
    /// </summary>
    public class InterfaceInfo
    {
        public InterfaceFormat InterfaceFormat { get; set; }
        public uint InterfaceValue { get; set; }
    }
}