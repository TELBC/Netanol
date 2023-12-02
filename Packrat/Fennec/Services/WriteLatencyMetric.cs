using System.Net;
using Fennec.Parsers;
using Fennec.Services;

namespace Fennec.Collectors;

public interface IWriteLatencyMetric
{
    /// <summary>
    ///     Adds latency to the _currentLatencies
    /// </summary>
    /// 
    void AddLatency(IPEndPoint remoteEndpoint, ParserType protocol, double latency);

    /// <summary>
    ///     Updates the properties for specified times
    /// </summary>S
    void UpdateWriteLatencyMetricsForStats();
}

public class WriteLatencyMetric : IWriteLatencyMetric
{
    private static readonly int _hours = 24;
    private static readonly int _timesPerMinit = 2;
    private static readonly int _count = _timesPerMinit * 60 * _hours;
    private static readonly int _period = 60 / _timesPerMinit;

    private readonly Dictionary<IPEndPoint, Dictionary<ParserType, WriteLatency[]>> _allWriteLatencies;
    private readonly Dictionary<IPEndPoint, Dictionary<ParserType, List<double>>> _currentLatencies;
    private readonly IMetricService _metricService;
    private readonly int _size = _count;
    private readonly Dictionary<string, int> _times;
    private int _lastModifiedPosition;
    private DateTime _lastModifiedTime = DateTime.UtcNow;
    private Dictionary<IPEndPoint,int> _totalSingleTraceReceived = new ();

    public WriteLatencyMetric(IMetricService metricService)
    {
        _metricService = metricService;
        _currentLatencies = new Dictionary<IPEndPoint, Dictionary<ParserType, List<double>>>();
        _allWriteLatencies = new Dictionary<IPEndPoint, Dictionary<ParserType, WriteLatency[]>>();
        _times = new Dictionary<string, int>
        {
            { "6Hours", _timesPerMinit * 60 * 6 },
            { "12Hours", _timesPerMinit * 60 * 12 },
            { "24Hours", _timesPerMinit * 60 * 24 }
        };
    }

    public void AddLatency(IPEndPoint remoteEndpoint, ParserType protocol, double latency)
    {
        if ((DateTime.UtcNow - _lastModifiedTime).TotalSeconds >= _period)
            CalculateWriteLatencyMetricForLastPeriod();
        AddLatencyToCurrentLatencies(remoteEndpoint, protocol, latency);
        AddLatencyToCurrentLatencies(remoteEndpoint, ParserType.All, latency);
    }

    public void UpdateWriteLatencyMetricsForStats()
    {
        var metric = _metricService.GetMetrics<WriteLatencyMetricsForStats>("WriteLatencyMetricsForStats");
        foreach (var endpoint in _allWriteLatencies.Keys)
        {
            if (!metric.MetricsByIpEndpoint.ContainsKey(endpoint))
                metric.MetricsByIpEndpoint.Add(endpoint, new WriteLatencyMetricsForStats.MetricsData());
            var metricsData = metric.MetricsByIpEndpoint[endpoint];
            metricsData.TotalSingleTracesReceived = _totalSingleTraceReceived[endpoint];
            foreach (var time in _times.Keys)
            {
                if (!metricsData.WriteLatencyOverTime.ContainsKey(time))
                    metricsData.WriteLatencyOverTime.Add(time, new List<WriteLatency>());
                if (!metricsData.ReceivedOverTime.ContainsKey(time))
                    metricsData.ReceivedOverTime.Add(time, 0);
                var periodReceived = 0;

                var writeLatenciesList = metricsData.WriteLatencyOverTime[time];
                writeLatenciesList.Clear();

                foreach (var protocol in _allWriteLatencies[endpoint].Keys)
                {
                    var latenciesArray = _allWriteLatencies[endpoint][protocol];

                    var toTake = _times[time];

                    // in case the last element in array has the same min and max
                    if (latenciesArray.Last().MinLatency == latenciesArray.Last().MaxLatency)
                    {
                        var frontStart = 0 < _lastModifiedPosition - toTake
                            ? 0
                            : _lastModifiedPosition - toTake;
                        var rearEnd = _lastModifiedPosition;
                        latenciesArray = latenciesArray.Skip(frontStart).Take(rearEnd).ToArray();
                    }
                    else
                    {
                        // in case we dont have to split into two 
                        if (_lastModifiedPosition - toTake > 0)
                        {
                            latenciesArray = latenciesArray.Skip(_lastModifiedPosition - toTake)
                                .Take(_lastModifiedPosition)
                                .ToArray();
                        }
                        // in case we have to take some WriteLatencies from the rear and from the front
                        else
                        {
                            var rearArray = latenciesArray.Skip(_lastModifiedPosition - toTake).Take(_size).ToArray();
                            var frontArray = latenciesArray.Skip(0).Take(_lastModifiedPosition).ToArray();
                            latenciesArray = rearArray.Concat(frontArray).ToArray();
                        }
                    }

                    if (latenciesArray != null)
                    {
                        var latenciesList = latenciesArray.ToList();
                        var minLatency = double.MaxValue;
                        double averageLatency = 0;
                        double medianLatency = 0;
                        double maxLatency = 0;
                        double q1Latency = 0;
                        double q3Latency = 0;
                        var singleTraceCount = 0;

                        foreach (var latency in latenciesArray)
                        {
                            if (latency.MinLatency == latency.MaxLatency) continue;
                            minLatency = Math.Min(minLatency, latency.MinLatency);
                            averageLatency += latency.AverageLatency;
                            medianLatency += latency.MedianLatency;
                            maxLatency = Math.Max(maxLatency, latency.MaxLatency);
                            q1Latency += latency.Q1Latency;
                            q3Latency += latency.Q3Latency;
                            singleTraceCount += latency.SingleTraceCount;
                        }

                        var index = latenciesArray.Length;
                        averageLatency /= index;
                        medianLatency /= index;
                        q1Latency /= index;
                        q3Latency /= index;

                        var metricsWriteLatency = new WriteLatency
                        {
                            AverageLatency = averageLatency,
                            MinLatency = minLatency,
                            MaxLatency = maxLatency,
                            MedianLatency = medianLatency,
                            Q1Latency = q1Latency,
                            Q3Latency = q3Latency,
                            SingleTraceCount = singleTraceCount,
                            Protocol = protocol.ToString()
                        };
                        if (protocol == ParserType.All)
                        {
                            periodReceived += singleTraceCount;
                            metricsData.ReceivedOverTime[time] = (uint)periodReceived;
                        }
                        writeLatenciesList.Add(metricsWriteLatency);
                    }
                }
            }
        }
    }

    private void AddLatencyToCurrentLatencies(IPEndPoint remoteEndpoint, ParserType protocol, double latency)
    {
        if (!_currentLatencies.TryGetValue(remoteEndpoint, out var endpointLatencies))
        {
            endpointLatencies = new Dictionary<ParserType, List<double>>();
            _currentLatencies[remoteEndpoint] = endpointLatencies;
        }

        if (!endpointLatencies.TryGetValue(protocol, out var latencyList))
        {
            latencyList = new List<double>();
            endpointLatencies[protocol] = latencyList;
        }

        latencyList.Add(latency);
    }

    private void CalculateWriteLatencyMetricForLastPeriod()
    {
        var currentTime = DateTime.UtcNow;

        while (currentTime - _lastModifiedTime > TimeSpan.FromSeconds(_period))
            foreach (var endpoint in _currentLatencies.Keys)
            {
                foreach (var protocol in _currentLatencies[endpoint].Keys)
                {
                    var latencies = _currentLatencies[endpoint][protocol];
                    var writeLatency = new WriteLatency();

                    if (latencies != null && latencies.Any())
                    {
                        var sortedLatencies = latencies.OrderBy(x => x).ToList();

                        writeLatency = new WriteLatency
                        {
                            Protocol = protocol.ToString(),
                            AverageLatency = latencies.Average(),
                            MaxLatency = latencies.Max(),
                            MinLatency = latencies.Min(),
                            MedianLatency = sortedLatencies[sortedLatencies.Count / 2],
                            Q1Latency = sortedLatencies[(int)(0.25 * (sortedLatencies.Count - 1))],
                            Q3Latency = sortedLatencies[(int)(0.75 * (sortedLatencies.Count - 1))],
                            SingleTraceCount = latencies.Count
                        };
                    }
                    else
                    {
                        writeLatency = new WriteLatency
                        {
                            Protocol = protocol.ToString(),
                            AverageLatency = 0,
                            MaxLatency = 0,
                            MinLatency = 0,
                            MedianLatency = 0,
                            Q1Latency = 0,
                            Q3Latency = 0,
                            SingleTraceCount = 0
                        };
                    }
                    if(!_totalSingleTraceReceived.ContainsKey(endpoint))
                        _totalSingleTraceReceived.Add(endpoint,0);
                    if(protocol == ParserType.All)
                        _totalSingleTraceReceived[endpoint] += latencies.Count;
                    
                    if (!_allWriteLatencies.TryGetValue(endpoint, out var endpointWriteLatencies))
                    {
                        endpointWriteLatencies = new Dictionary<ParserType, WriteLatency[]>();
                        _allWriteLatencies[endpoint] = endpointWriteLatencies;
                    }

                    // by default all take the first enum as Protocol in this case NetFlow9
                    if (!endpointWriteLatencies.TryGetValue(protocol, out var protocolWriteLatencyArray))
                    {
                        protocolWriteLatencyArray = new WriteLatency[_size];
                        endpointWriteLatencies[protocol] = protocolWriteLatencyArray;
                    }

                    protocolWriteLatencyArray[_lastModifiedPosition] = writeLatency;

                    latencies.Clear();
                }

                _lastModifiedPosition = (_lastModifiedPosition + 1) % _size;
                _lastModifiedTime += TimeSpan.FromSeconds(_period);
                currentTime = DateTime.UtcNow;
            }
    }
}

public struct WriteLatency
{
    public double AverageLatency;
    public double MaxLatency;
    public double MedianLatency;
    public double MinLatency;
    public string? Protocol;
    public double Q1Latency;
    public double Q3Latency;
    public int SingleTraceCount;
}

public class WriteLatencyMetricsForStats
{
    public Dictionary<IPEndPoint, MetricsData> MetricsByIpEndpoint = new();

    public class MetricsData
    {
        public Dictionary<string, uint> ReceivedOverTime = new();
        public int TotalSingleTracesReceived;
        public Dictionary<string, List<WriteLatency>> WriteLatencyOverTime = new();
    }
}