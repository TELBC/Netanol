using System.ComponentModel.DataAnnotations;

namespace Fennec.Options;

public class ElasticsearchOptions
{
    /// <summary>
    ///     The URI of the Elasticsearch instance.
    /// </summary>
    [Required]
    public string Uri { get; set; } = null!;

    /// <summary>
    ///     The username for the Elasticsearch user.
    /// </summary>
    public string? Username { get; set; } = null;

    /// <summary>
    ///     The password for the Elasticsearch user.
    /// </summary>
    public string? Password { get; set; } = null;
}