using Nexus.ViewModels;

namespace Nexus.Extensions;

public static class DisposableExtensions
{
    public static T AddTo<T>(this T viewModel)
        where T : ViewModel
    {
        return viewModel;
    }
}
