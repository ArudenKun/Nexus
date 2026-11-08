using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Volo.Abp;

namespace Nexus;

internal static class Program
{
    private static Task Main(string[] args)
    {
        return BuildAvaloniaApp().WithInterFont().StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure(() =>
            new App(
                AbpApplicationFactory.Create<NexusAvaloniaBrowserModule>(options =>
                {
                    options.UseAutofac();
                })
            )
        );
}
