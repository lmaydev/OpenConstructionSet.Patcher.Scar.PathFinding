using Microsoft.Toolkit.Mvvm.Input;
using OpenConstructionSet.Data;
using OpenConstructionSet.Mods.Context;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System.IO;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private const string ModFileName = ModName + ".mod";
        private const string ModName = "OCSP SCAR's pathfinding fix";
        private const string ReferenceModFileName = "SCAR's pathfinding fix.mod";
        private readonly IContextBuilder builder;
        private readonly ScarPathfindingFixPatcher patcher;
        private bool busy;

        public MainViewModel(IContextBuilder builder,
                             LoadOrderViewModel loadOrder,
                             InstallationSelectionViewModel installationSelection,
                             ScarPathfindingFixPatcher patcher)
        {
            this.builder = builder;

            InstallationSelection = installationSelection;

            LoadOrder = loadOrder;

            this.patcher = patcher;

            CreateMod = new AsyncRelayCommand(CreateModExecuteAsync);

            Messenger<Infrastructure.Messages.ShowMessageBox>.MessageRecieved += MessageRecieved;
        }

        public bool Busy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        public AsyncRelayCommand CreateMod { get; }

        public InstallationSelectionViewModel InstallationSelection { get; }

        public LoadOrderViewModel LoadOrder { get; }

        private async Task CreateModExecuteAsync()
        {
            var installation = InstallationSelection.SelectedInstallation;

            if (installation is null)
            {
                return;
            }

            Busy = true;

            try
            {
                var mods = LoadOrder.Mods.Where(m => m.Selected && m.Name != ReferenceModFileName && m.Name != ModFileName)
                                         .Select(m => m.Path)
                                         .ToList();

                if (mods.Count == 0)
                {
                    throw new Exception("No mods selected to patch");
                }

                var header = new Header(1, "LMayDev", "OpenConstructionSet compatibility patch to apply core values from SCAR's pathfinding fix to custom races");
                header.References.Add(ReferenceModFileName);
                header.Dependencies.AddRange(mods.Select(m => Path.GetFileName(m)));

                var options = new ModContextOptions(ModName,
                                                       throwIfMissing: false,
                                                       installation: installation,
                                                       header: header,
                                                       baseMods: mods);

                var context = await builder.BuildAsync(options);

                await patcher.PatchAsync(installation, context);

                await context.SaveAsync();

                var loadOrder = (await installation.ReadEnabledModsAsync()).ToList();

                loadOrder.RemoveAll(s => s.Equals(ModFileName, StringComparison.OrdinalIgnoreCase));

                loadOrder.Add(ModFileName);

                await installation.WriteEnabledModsAsync(loadOrder);

                Messenger<Infrastructure.Messages.ShowMessageBox>.Send(new("Mod created successfully", "Mod created!", MessageBoxImage.Information));

                Messenger<Refresh>.Send(new());
            }
            catch (Exception ex)
            {
                Messenger<Infrastructure.Messages.ShowMessageBox>.Send(new($"Failed to create mod:{Environment.NewLine}{ex.Message}", "Error!", MessageBoxImage.Error));
            }
            finally
            {
                Busy = false;
            }
        }

        private void MessageRecieved(Infrastructure.Messages.ShowMessageBox message)
        {
            Application.Current.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(
                   Application.Current.MainWindow,
                   message.Message,
                   message.Title,
                   message.Button,
                   message.Image));
        }
    }
}