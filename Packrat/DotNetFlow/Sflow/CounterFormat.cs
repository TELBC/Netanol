namespace DotNetFlow.Sflow
{
    /// <summary>
    /// The format of the upcoming counter record.
    /// Can be extended to include other counter types.
    /// </summary>
    public enum CounterFormat
    {
        GenericInterfaceCounters = 1,
        EthernetInterfaceCounters = 2,
        VlanCounters = 5,
        InfiniBandCounters = 9
    }
}