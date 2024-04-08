namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Base class for all counter records.
    /// </summary>
    public abstract class CounterRecord : IRecord
    {
        public Enterprise Enterprise { get; set; }
        public CounterFormat Format;
    }
}