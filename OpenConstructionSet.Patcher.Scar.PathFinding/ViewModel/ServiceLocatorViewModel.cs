using Microsoft.Extensions.DependencyInjection;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ServiceLocatorViewModel
    {
        public ServiceLocatorViewModel() =>
            MainViewModel = new ServiceCollection().AddOpenContructionSet()
                                                   .AddSingleton<InstallationSelectionViewModel>()
                                                   .AddSingleton<LoadOrderViewModel>()
                                                   .AddSingleton<MainViewModel>()
                                                   .AddSingleton<ScarPathfindingFixPatcher>()
                                                   .BuildServiceProvider()
                                                   .GetRequiredService<MainViewModel>();

        public MainViewModel MainViewModel { get; }
    }
}