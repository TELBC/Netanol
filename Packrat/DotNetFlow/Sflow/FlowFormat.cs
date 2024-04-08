namespace DotNetFlow.Sflow
{
    /// <summary>
    /// The format of the upcoming flow record.
    /// Can be extended to include other flow types.
    /// </summary>
    public enum FlowFormat
    {
        RawPacketHeader = 1,
        EthernetFrame = 2,
        IPv4Header = 3,
        IPv6Header = 4,
        ExtendedSwitchData = 1001,
        ExtendedRouterData = 1002,
        ExtendedGatewayData = 1003,
        ExtendedUserFlowData = 1004
    }
}