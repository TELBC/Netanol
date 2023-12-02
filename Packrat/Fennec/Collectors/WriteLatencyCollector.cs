using Fennec.Services;
using static System.Double;

namespace Fennec.Collectors;

public class WriteLatency
{
    public double MinLatency;
    public double AverageLatency;
    public double MedianLatency;
    public double MaxLatency;
    public double Q1Latency;
    public double Q3Latency;
    public int SingleTraceCount;
}

internal struct SingleTraceLatency
{
    public DateTime TimeSpan;
    public double MinLatency;
    public double AverageLatency;
    public double MedianLatency;
    public double MaxLatency;
    public double Q1Latency;
    public double Q3Latency;
    public int SingleTraceCount;
}

/// <summary>
/// Collects the Latencies of the processing and insertions of SingleTraces
/// </summary>
public interface IWriteLatencyCollector
{
    /// <summary>
    /// Inserts a new latency value into the latency list and calculates latency metrics if the time difference since the last entry is greater than or equal to a specified number of seconds.
    /// <summary>
    void InsertLatencyAndCalculate(double latency);
    
    /// <summary>
    /// Adds new Metrics to the MetricService.
    /// </summary>
    void GetLatencies();
}

public class WriteLatencyCollector : IWriteLatencyCollector
{
    private static readonly int Seconds = 10;
    private int _lastChangedArrayPosition;
    private readonly List<double> _latencyList;
    private readonly IMetricService _metricService;
    private static DateTime _startTime { get; set; }
    private DateTime _timeOfLastSingleTraceLatencyEntry { get; set; }
    private readonly SingleTraceLatency[] _singleTraceLatencyArray;

    public WriteLatencyCollector(IMetricService metricService)
    {
        _metricService = metricService;
        _startTime = DateTime.UtcNow;
        _timeOfLastSingleTraceLatencyEntry = DateTime.UtcNow;
        _lastChangedArrayPosition = 0;
        _latencyList = new List<double>();
        _singleTraceLatencyArray = new SingleTraceLatency[2880];
    }

    public void InsertLatencyAndCalculate(double latency)
    {
        var timeNow = _startTime == DateTime.UtcNow ? _startTime : DateTime.UtcNow;
        var timeDiff = timeNow - _timeOfLastSingleTraceLatencyEntry;

        if (timeDiff >= TimeSpan.FromSeconds(Seconds))
        {
            _timeOfLastSingleTraceLatencyEntry += TimeSpan.FromSeconds(Seconds);
            if (_latencyList.Count == 0)
            {
                AddNewLatencyEntry(_timeOfLastSingleTraceLatencyEntry, 0, 0, 0, 0, 0, 0, 0);
            }
            else
            {
                var orderedLatencies = new List<double>(_latencyList);
                orderedLatencies.Sort();
                AddNewLatencyEntry(
                    _timeOfLastSingleTraceLatencyEntry,
                    orderedLatencies.Min(),
                    orderedLatencies.Average(),
                    orderedLatencies[orderedLatencies.Count / 2],
                    orderedLatencies.Max(),
                    orderedLatencies[orderedLatencies.Count / 4],
                    orderedLatencies[orderedLatencies.Count * 3 / 4],
                    orderedLatencies.Count
                );
            }

            _latencyList.Clear();
        }

        _latencyList.Add(latency);
    }

    private void AddNewLatencyEntry(DateTime timeSpan, double minLatency, double averageLatency, double medianLatency,
        double maxLatency, double q1Latency, double q3Latency, int totalCount)
    {
        _singleTraceLatencyArray[_lastChangedArrayPosition] = new SingleTraceLatency
        {
            TimeSpan = timeSpan,
            MinLatency = minLatency,
            AverageLatency = averageLatency,
            MedianLatency = medianLatency,
            MaxLatency = maxLatency,
            Q1Latency = q1Latency,
            Q3Latency = q3Latency,
            SingleTraceCount = totalCount
        };
        _lastChangedArrayPosition = (_lastChangedArrayPosition + 1) % _singleTraceLatencyArray.Length;
    }

    public void GetLatencies()
    {
        GetLatencyForPeriod("LatencyLast6Hours", 720);
        GetLatencyForPeriod("LatencyLast12Hours", 1440);
        GetLatencyForPeriod("LatencyLast24Hours", 2880);
    }

    /// <summary>
    /// Gets the latencies for a specified time period (1 period is 30 seconds)
    /// </summary>
    private void GetLatencyForPeriod(string metricName, int period)
    {
        var currentSingleTraceArray = _singleTraceLatencyArray;
        var lastChangedArrayPosition = _lastChangedArrayPosition;
        var latency = _metricService.GetMetrics<WriteLatency>(metricName);
        var returnedLatency =
            GetAverageLatencyForPeriod(metricName, period, currentSingleTraceArray, lastChangedArrayPosition);

        latency.MinLatency = returnedLatency[metricName].MinLatency;
        latency.AverageLatency = returnedLatency[metricName].AverageLatency;
        latency.MedianLatency = returnedLatency[metricName].MedianLatency;
        latency.MaxLatency = returnedLatency[metricName].MaxLatency;
        latency.Q1Latency = returnedLatency[metricName].Q1Latency;
        latency.Q3Latency = returnedLatency[metricName].Q3Latency;
        latency.SingleTraceCount = returnedLatency[metricName].SingleTraceCount;
    }

    private Dictionary<string, WriteLatency> GetAverageLatencyForPeriod(string dictionaryName, int period,
        SingleTraceLatency[] currentSingleTraceArray, int lastChangedArrayPosition)
    {
        var minLatency = MaxValue;
        double averageLatency = 0;
        double medianLatency = 0;
        double maxLatency = 0;
        double q1Latency = 0;
        double q3Latency = 0;
        var singleTraceCount = 0;

        int frontStart;
        var frontEnd = 0;
        var rearStart = 0;
        int rearEnd;

        // if last element in list is "null" not null
        if (currentSingleTraceArray[^1].TimeSpan != new DateTime(0))
        {
            frontStart = lastChangedArrayPosition - period <= 0 ? 0 : lastChangedArrayPosition - period;
            frontEnd = frontStart != 0 ? lastChangedArrayPosition : 0;
            rearStart = frontEnd != 0 ? currentSingleTraceArray.Length - period + frontEnd : 0;
            rearEnd = rearStart != 0 ? currentSingleTraceArray.Length : lastChangedArrayPosition;
        }
        else
        {
            frontStart = lastChangedArrayPosition - period <= 0 ? 0 : lastChangedArrayPosition - period;
            rearEnd = rearStart != 0 ? currentSingleTraceArray.Length : lastChangedArrayPosition;
        }

        CalculateLatencyMetrics(currentSingleTraceArray, ref minLatency, ref averageLatency, ref medianLatency,
            ref maxLatency, ref q1Latency, ref q3Latency, ref singleTraceCount, frontStart, frontEnd);
        CalculateLatencyMetrics(currentSingleTraceArray, ref minLatency, ref averageLatency, ref medianLatency,
            ref maxLatency, ref q1Latency, ref q3Latency, ref singleTraceCount, rearStart, rearEnd);

        var index = period >= lastChangedArrayPosition++ ? lastChangedArrayPosition++ : period;
        averageLatency /= index;
        medianLatency /= index;
        q1Latency /= index;
        q3Latency /= index;

        var latency = new WriteLatency
        {
            MinLatency = IsNaN(minLatency) ? 0 : minLatency,
            AverageLatency = IsNaN(averageLatency) ? 0 : averageLatency,
            MedianLatency = IsNaN(medianLatency) ? 0 : medianLatency,
            MaxLatency = IsNaN(maxLatency) ? 0 : maxLatency,
            Q1Latency = IsNaN(q1Latency) ? 0 : q1Latency,
            Q3Latency = IsNaN(q3Latency) ? 0 : q3Latency,
            SingleTraceCount = singleTraceCount
        };

        return new Dictionary<string, WriteLatency>(1) { { dictionaryName, latency } };
    }

    private static void CalculateLatencyMetrics(SingleTraceLatency[] currentSingleTraceArray, ref double minLatency,
        ref double averageLatency, ref double medianLatency, ref double maxLatency, ref double q1Latency,
        ref double q3Latency, ref int singleTraceCount, int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            minLatency = Math.Min(minLatency, currentSingleTraceArray[i].MinLatency);
            averageLatency += currentSingleTraceArray[i].AverageLatency;
            medianLatency += currentSingleTraceArray[i].MedianLatency;
            maxLatency = Math.Max(maxLatency, currentSingleTraceArray[i].MaxLatency);
            q1Latency += currentSingleTraceArray[i].Q1Latency;
            q3Latency += currentSingleTraceArray[i].Q3Latency;
            singleTraceCount += currentSingleTraceArray[i].SingleTraceCount;
        }
    }
}