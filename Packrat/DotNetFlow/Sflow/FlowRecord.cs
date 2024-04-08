namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Base class for all flow records.
    /// </summary>
    public abstract class FlowRecord : IRecord
    {
        public Enterprise Enterprise { get; set; }
        public FlowFormat Format;
    }
}