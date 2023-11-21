namespace Fennec.Services;

/// <summary>
/// Provides Metrics to frontend.
/// </summary>
public interface IMetricService
{    
    /// <summary>
    /// Gets object of a specified name from the main dictionary.
    /// If the object name do not exist, a new instance is created and added to the dictionary.
    /// </summary>
    Task<Dictionary<string, object>> GetAllMetrics();
    
    /// <summary>
    /// Gets all metrics from the main dictionary.
    /// </summary>
    /// <returns>Dictionary of object names and the object itself.</returns>
    T GetMetrics<T>(string name) where T: new();
}

public class MetricService : IMetricService
{
    private readonly Dictionary<string, object> _mainDictionary;
    
    public MetricService()
    {
        _mainDictionary = new Dictionary<string, object>();
    }

    public T GetMetrics<T>(string name) where T : new()
    {
        if (!_mainDictionary.ContainsKey(name))
            _mainDictionary.Add(name, new T());
        return (T) _mainDictionary[name];
    }
    
    public async Task<Dictionary<string, object>> GetAllMetrics()
    {
        return _mainDictionary;
    }
}
