using System.Net;
using Fennec.Options;
using Fennec.Services;
using Microsoft.Extensions.Options;

namespace Fennec.Metrics;

public interface IFlowImporterMetric
{
    /// <summary>
    /// Adds a Flow into the FlowImporter.
    /// </summary>
    /// <param name="parsed"></param>
    /// <param name="endPoint"></param>
    /// <param name="receivedByteCount"></param>
    /// <param name="transmittedBytes"></param>
    /// <param name="transmittedPackets"></param>
    void AddFlowImport(bool parsed, IPEndPoint endPoint, int receivedByteCount, long transmittedBytes, long transmittedPackets);

    /// <summary>
    /// Sums the entries entered in a specified period.
    /// </summary>
    void SumLastPeriod();
    
    /// <summary>
    /// Updates the metric for the FlowSeries.
    /// </summary>
    void UpdateFlowSeriesMetric();

    /// <summary>
    /// Updates the metric for the FlowGeneral.
    /// </summary>
    void UpdateFlowGeneralMetric();
}

public class FlowImporterMetric : IFlowImporterMetric
{
    private readonly Dictionary<IPEndPoint, IpEndPointsData> _endPointsData; // stores FlowImport data continuously
    private readonly FlowImporterDataSeries[] _flowImporterData;
    private readonly IMetricService _metricService;
    private readonly Dictionary<IPEndPoint, IpEndPointsData> _tempFlowImports; // to store FlowImport data since last period
    private readonly int ArraySize;
    private readonly ILogger _logger;

    private int _nextModifiedPosition;

    public FlowImporterMetric(IMetricService metricService, IOptions<FlowImporterMetricsOptions> options, ILogger logger)
    {
        _metricService = metricService;
        _tempFlowImports = new Dictionary<IPEndPoint, IpEndPointsData>();
        _endPointsData = new Dictionary<IPEndPoint, IpEndPointsData>();
        ArraySize = (int)(options.Value.FlowSavePeriod / options.Value.TraceSummationPeriod);
        _flowImporterData = new FlowImporterDataSeries[ArraySize];
        _logger = logger;
    }

    public void AddFlowImport(bool parsed, IPEndPoint endPoint, int receivedByteCount, long transmittedPackets, long transmittedBytes)
    {
        if (_tempFlowImports.ContainsKey(endPoint))
        {
            var temp = _tempFlowImports[endPoint];
            temp.ReceivedPacketCount++;
            temp.ReceivedByteCount += receivedByteCount;
            temp.TransmittedPacketCount += transmittedPackets;
            temp.TransmittedByteCount += transmittedBytes;
            if (parsed)
                temp.SuccessfullyParsedPacket++;
            else
                temp.FailedParsedPacket++;
            _tempFlowImports[endPoint] = temp;
        }
        else
        {
            _tempFlowImports.Add(endPoint,
                new IpEndPointsData(1, 
                    receivedByteCount, 
                    transmittedPackets, 
                    transmittedBytes, 
                    parsed ? 1 : 0, 
                    parsed ? 0 : 1));
        }
    }

    public void SumLastPeriod()
    {
        _flowImporterData[_nextModifiedPosition] = new FlowImporterDataSeries
        {
            DateTime = DateTime.UtcNow,
            Endpoints = new Dictionary<IPEndPoint, int>(_tempFlowImports.ToDictionary(pair => pair.Key,
                pair => pair.Value.ReceivedPacketCount))
        };

        foreach (var endpoint in _tempFlowImports)
        {
            if (_endPointsData.ContainsKey(endpoint.Key))
            {
                var existingData = _endPointsData[endpoint.Key];
                existingData.ReceivedPacketCount += _tempFlowImports[endpoint.Key].ReceivedPacketCount;
                existingData.ReceivedByteCount += _tempFlowImports[endpoint.Key].ReceivedByteCount;
                existingData.TransmittedPacketCount += _tempFlowImports[endpoint.Key].TransmittedPacketCount;
                existingData.TransmittedByteCount += _tempFlowImports[endpoint.Key].TransmittedByteCount;
                existingData.SuccessfullyParsedPacket += _tempFlowImports[endpoint.Key].SuccessfullyParsedPacket;
                existingData.FailedParsedPacket += _tempFlowImports[endpoint.Key].FailedParsedPacket;
                _endPointsData[endpoint.Key] = existingData;
            }
            else
            {
                _endPointsData.Add(endpoint.Key, new IpEndPointsData(
                    _tempFlowImports[endpoint.Key].ReceivedPacketCount,
                    _tempFlowImports[endpoint.Key].ReceivedByteCount,
                    _tempFlowImports[endpoint.Key].TransmittedPacketCount,
                    _tempFlowImports[endpoint.Key].TransmittedByteCount,
                    _tempFlowImports[endpoint.Key].SuccessfullyParsedPacket,
                    _tempFlowImports[endpoint.Key].FailedParsedPacket));
            } 
        }
        
        _tempFlowImports.Clear();
        _nextModifiedPosition = (_nextModifiedPosition + 1) % ArraySize;
    }
    
    public void UpdateFlowSeriesMetric()
    {
        var metrics = _metricService.GetMetrics<FlowSeriesData>("FlowSeriesData");
        if (_flowImporterData != null)
        {
            metrics.FlowImporterDataSeries = _flowImporterData
                .Where(fid => fid != null && fid.DateTime != DateTime.MinValue)
                .ToArray();
        }
        else
        {
            metrics.FlowImporterDataSeries = Array.Empty<FlowImporterDataSeries>();
        }
        _logger.Debug("Updated FlowImports... It now contains {Size} IP entries", _flowImporterData.Count(fid => fid != null && fid.DateTime != null));
    }

    public void UpdateFlowGeneralMetric()
    {
        var metrics = _metricService.GetMetrics<FlowGeneraData>("FlowGeneralData");
        metrics.EndPointsData = _endPointsData;
    }
}

public class FlowImporterTimer : BackgroundService
{
    private readonly IFlowImporterMetric _flowImporterMetric;
    private readonly FlowImporterMetricsOptions _metricsOptions;

    public FlowImporterTimer(IFlowImporterMetric flowImporterMetric, IOptions<FlowImporterMetricsOptions> options)
    {
        _metricsOptions = options.Value;
        _flowImporterMetric = flowImporterMetric;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _flowImporterMetric.SumLastPeriod();
            await Task.Delay(_metricsOptions.TraceSummationPeriod, stoppingToken);
        }
    }
}

public struct IpEndPointsData
{
    public IpEndPointsData(int receivedPacketCount, int receivedByteCount, long transmittedPacketCount,
        long transmittedByteCount, int successfullyParsedPacket, int failedParsedPacket)
    {
        ReceivedPacketCount = receivedPacketCount;
        ReceivedByteCount = receivedByteCount;
        TransmittedPacketCount = transmittedPacketCount;
        TransmittedByteCount = transmittedByteCount;
        SuccessfullyParsedPacket = successfullyParsedPacket;
        FailedParsedPacket = failedParsedPacket;
    }

    public int ReceivedPacketCount { get; set; }
    public int ReceivedByteCount { get; set; }
    public long TransmittedPacketCount { get; set; }
    public long TransmittedByteCount { get; set; }
    public int SuccessfullyParsedPacket { get; set; }
    public int FailedParsedPacket { get; set; }
}

public class FlowImporterDataSeries
{
    public DateTime DateTime { get; init; }
    public Dictionary<IPEndPoint, int> Endpoints { get; set; }
}

public class FlowSeriesData
{
    public FlowImporterDataSeries[] FlowImporterDataSeries { get; set; }
}

public class FlowGeneraData
{
    public Dictionary<IPEndPoint, IpEndPointsData> EndPointsData { get; set; }
}