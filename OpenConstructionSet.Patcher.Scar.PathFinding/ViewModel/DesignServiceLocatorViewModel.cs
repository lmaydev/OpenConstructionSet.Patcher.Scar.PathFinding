using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using OpenConstructionSet.Patcher.Scar.PathFinding.Design;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class DesignServiceLocatorViewModel
    {
        public DesignServiceLocatorViewModel()
        {
            MainViewModel = new ServiceCollection().AddOpenContructionSet()
                                                  .AddSingleton(context => App.Fixture.Create<IOcsDiscoveryService>())
                                                  .AddSingleton<InstallationSelectionViewModel>()
                                                  .AddSingleton<LoadOrderViewModel>()
                                                  .AddSingleton<MainViewModel>()
                                                  .AddSingleton<ScarPathfindingFixPatcher>()
                                                  .BuildServiceProvider()
                                                  .GetRequiredService<MainViewModel>();
        }

        public MainViewModel MainViewModel { get; private set; }
    }
}
