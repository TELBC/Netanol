namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents an interface for all sFlow records.
    /// </summary>
    public interface IRecord
    {
        Enterprise Enterprise { get; set; }
    }
}