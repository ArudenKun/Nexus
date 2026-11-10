using Fluently.Abstractions.Theme;

namespace Nexus.Events;

public sealed record ThemeChangedEvent(ApplicationTheme Theme);
