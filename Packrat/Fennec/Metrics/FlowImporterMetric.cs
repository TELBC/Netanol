using System.Net;
using Fennec.Services;

namespace Metrics;

public interface IFlowImporterMetric
{
    /// <summary>
    ///     Adds the endpoint the the FlowImporter and increases the counter of how many we have received from that endpoint by
    ///     one.
    /// </summary>
    void AddFlowImport(IPEndPoint endPoint);
    
    /// <summary>
    ///     Updates the metric in the MetricService for FlowImporter
    /// </summary>
    void UpdateMetric();
}

public class FlowImporterMetric : IFlowImporterMetric
{
    private const int arraySize = 5760;
    private const int periodInMinute = 1; // ex: periodInMinute of 1 will do the calculation every Minute

    private readonly FlowImporterDataSeries[] _flowImporterData; // to store all FlowImportCounts 
    private readonly IMetricService _metricService;

    private readonly PeriodicTimer _periodicTimer;

    private readonly Dictionary<IPEndPoint, int> tempFlowImports; // to store FlowImportCount in last xx seconds
    private int _nextModifiedPosition;

    public FlowImporterMetric(IMetricService metricService)
    {
        tempFlowImports = new Dictionary<IPEndPoint, int>();
        _flowImporterData = new FlowImporterDataSeries[arraySize];
        _nextModifiedPosition = 0;
        _metricService = metricService;
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(periodInMinute));

        StartPeriodicTask(); // to start the periodicProcess
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit; // to safely dispose once application ends
    }

    public void AddFlowImport(IPEndPoint endPoint)
    {
        tempFlowImports[endPoint] = tempFlowImports.TryGetValue(endPoint, out var value) ? value + 1 : 1;
    }

    public void UpdateMetric()
    {
        var metrics = _metricService.GetMetrics<FlowImporterData>("FlowImporterData");
        metrics.FlowImporterDataSeries = _flowImporterData.Where(fid => fid.DateTime != DateTime.MinValue).ToArray();
    }

    private void StartPeriodicTask()
    {
        Task.Run(async () =>
        {
            while (await _periodicTimer.WaitForNextTickAsync()) 
                SumLastPeriod();
        });
    }

    private void SumLastPeriod()
    {
        _flowImporterData[_nextModifiedPosition] = new FlowImporterDataSeries
        {
            DateTime = DateTime.UtcNow,
            Endpoints = new Dictionary<IPEndPoint, int>(tempFlowImports)
        };

        tempFlowImports.Clear();
        _nextModifiedPosition = (_nextModifiedPosition + 1) % arraySize;
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        _periodicTimer?.Dispose();
    }
}

public struct FlowImporterDataSeries
{
    public DateTime DateTime { get; init; }
    public Dictionary<IPEndPoint, int> Endpoints { get; set; }
}

public class FlowImporterData
{
    public FlowImporterDataSeries[] FlowImporterDataSeries;
}