using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexus.Core.System;

/// <summary>
/// Thrown by <see cref="System.Platform.Current"/> when the app declared its supported operating systems with
/// <see cref="Platform.DeclareSupportedOperatingSystems"/> and is running on a different one.
/// </summary>
public sealed class UnsupportedOperatingSystemException : PlatformNotSupportedException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsupportedOperatingSystemException"/> class.
    /// </summary>
    /// <param name="platform">The detected operating system.</param>
    /// <param name="supportedPlatformTypes">The operating systems the app declared it supports.</param>
    public UnsupportedOperatingSystemException(
        PlatformType platform,
        IReadOnlySet<PlatformType> supportedPlatformTypes
    )
        : base(
            $"{platform} isn't supported. Supported: {string.Join(", ", supportedPlatformTypes.Order())}."
        )
    {
        Platform = platform;
        SupportedPlatformTypes = supportedPlatformTypes;
    }

    /// <summary>
    /// Gets the detected operating system.
    /// </summary>
    public PlatformType Platform { get; }

    /// <summary>
    /// Gets the operating systems the app declared it supports.
    /// </summary>
    public IReadOnlySet<PlatformType> SupportedPlatformTypes { get; }
}
