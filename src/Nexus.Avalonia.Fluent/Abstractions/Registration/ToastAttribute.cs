using Nexus.Fluent.Abstractions.Toasts;

namespace Nexus.Fluent.Abstractions.Registration;

/// <summary>
///     Registers this toast as transient by the generated <c>AddViewModels()</c>: a new instance every time it is shown by
///     its type.
///     The class must implement <see cref="IToast" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ToastAttribute : Attribute;
