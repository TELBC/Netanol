namespace Fennec.Options;

/// <summary>
///     Configuration for the option to use tag.
/// </summary>
public class TagsRequestOptions
{
    /// <summary>
    ///     Enable or disable the tag service.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Configuration for the requesting of the tags.
    /// </summary>
    public VmWareRequestConfigOptions VmWareRequest { get; set; }
}

/// <summary>
///     Configuration for requesting tags.
/// </summary>
public class VmWareRequestConfigOptions
{
    /// <summary>
    ///     The target's machine IP address (IPv4).
    /// </summary>
    public string VmWareTargetAddress { get; set; }

    /// <summary>
    ///     Credentials for the VMWare API.
    /// </summary>
    public VmWareCredentialsOptions VmWareCredentials { get; set; }

    /// <summary>
    ///     Paths for getting certain information from the Vmware API.
    /// </summary>
    public VmWareApiPathsOptions VmWareApiPaths { get; set; }

    /// <summary>
    ///     Defines how many request can be send per second to target machine
    /// </summary>
    public int MaxRequestPerSecond { get; set; } 
}

/// <summary>
///     Credentials for the VMWare API.
/// </summary>
public class VmWareCredentialsOptions
{
    /// <summary>
    ///     The Username of the VMware client for the API.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     The password of the VMware client for the API.
    /// </summary>
    public string Password { get; set; }
}

/// <summary>
///     Paths for getting certain information from the Vmware API.
/// </summary>
public class VmWareApiPathsOptions
{
    /// <summary>
    ///     Path for getting a session token.
    /// </summary>
    public string SessionPath { get; set; }

    /// <summary>
    ///     Path for getting all machines with their tag.
    /// </summary>
    public string TaggingAssociationsPath { get; set; }
}