using Nexus.Fluent.Abstractions.Theme;

namespace Nexus.Events;

public sealed record ThemeChangedEvent(ApplicationTheme Theme);
