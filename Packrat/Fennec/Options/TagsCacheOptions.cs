﻿namespace Fennec.Options;

/// <summary>
/// Options for the Tags cache.
/// </summary>
public class TagsCacheOptions
{
    /// <summary>
    /// Refresh Period represented as string
    /// </summary>
    public TimeSpan RefreshPeriod { get; set; } = TimeSpan.FromHours(12);
} 