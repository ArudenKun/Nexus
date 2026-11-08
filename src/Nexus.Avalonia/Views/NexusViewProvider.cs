using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fluently;
using Volo.Abp.DynamicProxy;

namespace Nexus.Views;

/// <summary>
/// Wraps Fluently's <see cref="IViewProvider"/> to resolve views for Castle/ABP
/// dynamic-proxy ViewModels, which Fluently's <c>ViewRegistry</c> cannot match
/// because it keys on <c>viewModel.GetType()</c> (the generated proxy type).
/// </summary>
public sealed class NexusViewProvider : IViewProvider
{
    private static readonly FieldInfo FactoriesField =
        typeof(ViewRegistry).GetField("Factories", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException(
            "ViewRegistry.Factories not found — Fluently API changed."
        );

    private static readonly Type ViewModelScopesType =
        typeof(ViewRegistry)
            .Assembly.GetTypes()
            .FirstOrDefault(t => t is { Name: "ViewModelScopes", IsSealed: true, IsPublic: false })
        ?? throw new InvalidOperationException(
            "ViewModelScopes type not found — Fluently API changed."
        );

    private static readonly MethodInfo ForMethod =
        ViewModelScopesType.GetMethod(
            "For",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        )
        ?? throw new InvalidOperationException(
            "ViewModelScopes.For not found — Fluently API changed."
        );

    // Snapshot of ViewRegistry.Factories keyed by AssemblyQualifiedName so that
    // identically-named ViewModels from different plugin assemblies don't collide.
    // ViewRegistry is populated at assembly load and never mutates afterwards.
    private static readonly Lazy<IReadOnlyDictionary<string, Delegate>> FactoriesByFullName = new(
        () =>
        {
            var dict = new ConcurrentDictionary<string, Delegate>(StringComparer.Ordinal);

            if (FactoriesField.GetValue(null) is not IDictionary raw)
                return dict;
            foreach (DictionaryEntry entry in raw)
            {
                if (entry is not { Key: Type key, Value: Delegate factory })
                    continue;
                var aqn = key.GetFullNameWithAssemblyName();
                if (!string.IsNullOrEmpty(aqn))
                    dict.TryAdd(aqn, factory);
            }

            return dict;
        }
    );

    private readonly IViewProvider _decorated;
    private readonly IServiceProvider _serviceProvider;

    public NexusViewProvider(IViewProvider decorated, IServiceProvider serviceProvider)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public bool CanProvide(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (!ProxyHelper.IsProxy(viewModel))
            return _decorated.CanProvide(viewModel);

        var realType = ProxyHelper.GetUnProxiedType(viewModel);
        return FactoriesByFullName.Value.ContainsKey(GetKey(realType));
    }

    public IViewFor<object> GetView(object viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (!ProxyHelper.IsProxy(viewModel))
            return _decorated.GetView(viewModel);

        var realType = ProxyHelper.GetUnProxiedType(viewModel);

        if (!FactoriesByFullName.Value.TryGetValue(GetKey(realType), out var factory))
        {
            throw new InvalidOperationException(
                $"No view factory registered for {realType.AssemblyQualifiedName} "
                    + $"(proxy for {viewModel.GetType().FullName})."
            );
        }

        var services = GetViewModelScope(viewModel);

        object? view;
        try
        {
            view = factory.DynamicInvoke(services, viewModel);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            // Surface the real factory exception instead of the reflection wrapper.
            throw new InvalidOperationException(
                $"View factory for {realType.GetFullNameWithAssemblyName()} threw an exception.",
                ex.InnerException
            );
        }

        return view as IViewFor<object>
            ?? throw new InvalidOperationException(
                $"Factory for {realType.AssemblyQualifiedName} returned "
                    + $"{view?.GetType().FullName ?? "null"}, which is not {nameof(IViewFor<>)}."
            );
    }

    private static string GetKey(Type type) => type.GetFullNameWithAssemblyName();

    /// <summary>
    /// Resolves the DI scope the given ViewModel instance was created from
    /// by calling <c>ViewModelScopes.For(viewModel)</c> on the DI-registered instance.
    /// </summary>
    private IServiceProvider GetViewModelScope(object viewModel)
    {
        var scopes = _serviceProvider.GetService(ViewModelScopesType);
        if (scopes is null)
        {
            // ViewModelScopes isn't reachable — fall back to root. Views that depend
            // on scoped services will resolve from root; acceptable as a safe default.
            return _serviceProvider;
        }

        try
        {
            var scopedServiceProvider = (IServiceProvider?)ForMethod.Invoke(scopes, [viewModel]);
            return scopedServiceProvider ?? _serviceProvider;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw new InvalidOperationException(
                $"ViewModelScopes.For threw for {viewModel.GetType().FullName}.",
                ex.InnerException
            );
        }
    }
}
