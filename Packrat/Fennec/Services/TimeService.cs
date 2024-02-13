namespace Fennec.Services;

public interface ITimeService
{
    /// <summary>
    /// The current time. 
    /// </summary>
    public DateTime Now { get; }
}

public class TimeService : ITimeService
{
     public DateTime Now => DateTime.Now;
}