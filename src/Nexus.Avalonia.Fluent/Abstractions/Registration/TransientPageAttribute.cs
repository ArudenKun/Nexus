using Nexus.Fluent.Abstractions.Navigation;

namespace Nexus.Fluent.Abstractions.Registration;

/// <summary>
///     Registers this page as transient by the generated <c>AddViewModels()</c>: a new instance on every navigation to it.
///     The class must implement <see cref="INavigableViewModel" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TransientPageAttribute : Attribute;
