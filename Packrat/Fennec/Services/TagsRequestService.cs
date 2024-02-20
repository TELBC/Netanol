using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using Fennec.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fennec.Services;

/// <summary>
///     Request the latest tag associations from target machine.
/// </summary>
public interface ITagsRequestService
{
    /// <summary>
    ///     Request and return the tags associated with every IP.
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<IPAddress, List<string>>> GetLatestTagsAndIps();
}

public class TagsRequestService : ITagsRequestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _log;
    private readonly VmWareRequestConfigOptions _options;
    private readonly Dictionary<string, string> _tags = new();
    private readonly List<VmWareMachine> _vmWareMachines = new();

    public TagsRequestService(IHttpClientFactory httpClientFactory, IOptions<TagsRequestOptions> options,
        ILogger log)
    {
        _log = log;
        _httpClient = httpClientFactory.CreateClient("VmWareApiClient");
        _options = options.Value.VmWareRequest;
    }

    public async Task<Dictionary<IPAddress, List<string>>> GetLatestTagsAndIps()
    {
        await RequestSessionTokenAsync();
        await RequestTagsAsync();
        await RequestIpAddressesAsync();
        return await RequestTagDetailsAsync();
    }

    /// <summary>
    ///     Request SessionToken from target machine
    /// </summary>
    /// <exception cref="InvalidCredentialException">Thrown when the given credentials are incorrect</exception>
    private async Task RequestSessionTokenAsync()
    {
        _log.Debug("Acquiring session token for target machine {TargetMachine}",
            _options.VmWareTargetAddress);
        var response = await _httpClient.PostAsync(_options.VmWareApiPaths.SessionPath, new StringContent(string.Empty));

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidCredentialException(
                $"The provided credentials for ´{_options.VmWareTargetAddress}´ were incorrect.");
        response.EnsureSuccessStatusCode();

        var sessionId = await response.Content.ReadAsStringAsync();

        _httpClient.DefaultRequestHeaders.Remove("Cookie");
        _httpClient.DefaultRequestHeaders.Add("Cookie", $"vmware-api-session-id={sessionId}");

        _log.Debug("SessionToken acquired for target machine {TargetMachine}", _options.VmWareTargetAddress);
    }

    /// <summary>
    ///     Sends a request with session id and request all machines with a tag
    /// </summary>
    private async Task RequestTagsAsync()
    {
        var response = await _httpClient.GetAsync(_options.VmWareApiPaths.TaggingAssociationsPath);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(result);
        if (apiResponse == null)
            throw new InvalidDataException("The Vmware server returned a response which could not be parsed correctly.");

        _vmWareMachines.Clear();
        _vmWareMachines.AddRange(apiResponse.Associations.Select(association => new VmWareMachine
        {
            Id = association.Object.Id,
            Tag = new List<string> { association.Tag },
            IpAddresses = new List<IPAddress>()
        }));
    }

    /// <summary>
    ///     Sends a request to the addresses of the machine.
    /// </summary>
    private async Task RequestIpAddressesAsync()
    {
        var batchSize = _options.MaxRequestPerSecond;
        var vmWareMachines = _vmWareMachines
            .Where(vm => vm.Id.StartsWith("v"))
            .ToList();

        const int delayMilliseconds = 1000;

        for (var i = 0; i < vmWareMachines.Count; i += batchSize)
        {
            var batch = vmWareMachines.Skip(i).Take(batchSize);

            var stopwatch = Stopwatch.StartNew();

            var tasks = batch
                .Select(vm => HandleRequest(vm.Id))
                .ToList();

            await Task.WhenAll(tasks);

            stopwatch.Stop();
            
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds < delayMilliseconds)
            {
                await Task.Delay(delayMilliseconds - (int)elapsedMilliseconds);
            }
        }
        
        return;

        async Task HandleRequest(string id)
        {
            try
            {
                await ProcessVmAsync(id);
            }
            catch (Exception e)
            {
                _log.ForContext("Exception", e)
                    .ForContext("Vm-Id", id)
                    .Warning("Could not find the interfaces for Vm-Id {Id}... It is apparently " +
                             "wrongly formatted | {ExceptionName}: {ExceptionMessage}", id,e.GetType().Name, e.Message);
            }
        }
    }

    /// <summary>
    ///     Looks up the ips for a specific vm id
    /// </summary>
    /// <param name="id"></param>
    private async Task ProcessVmAsync(string id)
    {
        var tempPath = $"/api/vcenter/vm/{id}/guest/networking/interfaces";
        var response = await _httpClient.GetAsync(tempPath);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var ipAddressInfos = JsonConvert.DeserializeObject<List<IpAddressInfo>>(jsonResponse);
            if (ipAddressInfos == null)
                throw new InvalidDataException("The Vmware server returned an invalid response");

            foreach (var ipAddressInfo in ipAddressInfos)
            {
                var matchingVm = _vmWareMachines.FirstOrDefault(mc => mc.Id == id);
                if (matchingVm?.IpAddresses == null) continue;

                foreach (var ipAddress in ipAddressInfo.Ip.IpAddresses)
                    if (IPAddress.TryParse(ipAddress.IpAddressValue, out var parsedIpAddress) &&
                        parsedIpAddress.AddressFamily == AddressFamily.InterNetwork)
                        matchingVm.IpAddresses.Add(parsedIpAddress);
            }
        }
    }

    /// <summary>
    ///     Sends a request to get the latest label names and change the vm tags for the actual named and not their ids.
    /// </summary>
    private async Task<Dictionary<IPAddress, List<string>>> RequestTagDetailsAsync()
    {
        _tags.Clear();
        var batchSize = _options.MaxRequestPerSecond;
        
        var tasks = new List<Task>();
        
        foreach (var vm in _vmWareMachines)
        {
            foreach (var tag in vm.Tag)
            {
                if (_tags.ContainsKey(tag)) continue;
                tasks.Add(HandleRequest(tag));

                if (tasks.Count < batchSize) continue;
                await Task.WhenAll(tasks);
                tasks.Clear();
                await Task.Delay(1000);
            }
        }


        foreach (var vm in _vmWareMachines)
            for (var i = 0; i < vm.Tag.Count; i++)
                if (_tags.TryGetValue(vm.Tag[i], out var newTag))
                    vm.Tag[i] = newTag;

        var ipTagDict = new Dictionary<IPAddress, List<string>>();

        foreach (var vm in _vmWareMachines)
        foreach (var ipAddress in vm.IpAddresses)
        {
            if (!ipTagDict.ContainsKey(ipAddress))
                ipTagDict[ipAddress] = new List<string>();
            ipTagDict[ipAddress].AddRange(vm.Tag);
        }

        return ipTagDict;

        async Task ProcessTagAsync(string tag)
        {
            var response = await _httpClient.GetAsync($"https://{_options.VmWareTargetAddress}/api/cis/tagging/tag/{tag}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tagInfo = JsonConvert.DeserializeObject<TagInfo>(jsonResponse);
            if (tagInfo == null)
                throw new InvalidDataException("The Vmware server returned invalid data");

            if (!_tags.ContainsKey(tag))
                _tags.Add(tag, tagInfo.Name);
        }

        async Task HandleRequest(string tag)
        {
            try
            {
                await ProcessTagAsync(tag);
            }
            catch (Exception e)
            {
                _log.ForContext("Exception", e)
                    .ForContext("Tag", tag)
                    .Warning("Could not find name for Tag.. It is apparently | {ExceptionName}: {ExceptionMessage}", e.GetType().Name, e.Message);
            }
        }
    }
}

/// Helper classes
/// Needed for parsing the API responses

[JsonObject]
internal class Object
{
    public string Id { get; set; } = default!;
}

[JsonObject]
internal class Association
{
    public string Tag { get; set; } = default!;
    
    public Object Object { get; set; } = default!;
}

[JsonObject]
internal class ApiResponse
{
    public List<Association> Associations { get; set; } = default!;
}
[JsonObject]
internal class VmWareMachine
{
    public string Id { get; set; } = default!; 
    public List<string> Tag { get; set; } = default!;
    public List<IPAddress> IpAddresses { get; set; } = default!;
}

[JsonObject]
internal class IpAddressInfo
{
    [JsonProperty("ip")]
    public IpDetails Ip { get; set; } = default!;
}

internal class IpDetails
{
    [JsonProperty("ip_addresses")]
    public List<IpAddress> IpAddresses { get; set; } = new ();
}

internal class IpAddress
{
    [JsonProperty("ip_address")]
    public string IpAddressValue { get; set; } = default!;
}

[JsonObject]
internal class TagInfo
{
    public string Name { get; set; } = default!;
}