using OpenConstructionSet.Data;
using OpenConstructionSet.Models;
using OpenConstructionSet.Models.Enums;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System.IO;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private const string ModFileName = ModName + ".mod";
        private const string ModName = "OCSP - SCAR's pathfinding fix";
        private const string ReferenceModFileName = "SCAR's pathfinding fix.mod";
        private readonly IOcsBuilder builder;
        private readonly IOcsInstallationService installationService;
        private readonly ScarPathfindingFixPatcher patcher;
        private bool busy;

        public MainViewModel(IOcsBuilder builder,
                             IOcsInstallationService installationService,
                             InstallationSelectionViewModel installationSelection,
                             LoadOrderViewModel loadOrder,
                             ScarPathfindingFixPatcher patcher)
        {
            this.builder = builder;
            this.installationService = installationService;

            InstallationSelection = installationSelection;

            LoadOrder = loadOrder;

            this.patcher = patcher;

            CreateMod = new RelayCommand(StartCreateMod, CanCreateMod);
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

        public RelayCommand CreateMod { get; }
        public InstallationSelectionViewModel InstallationSelection { get; }
        public LoadOrderViewModel LoadOrder { get; }

        private bool CanCreateMod() => !Busy && LoadOrder.Mods.Any(m => m.Selected);

        private void CreateModExecute()
        {
            try
            {
                var mods = LoadOrder.Mods.Where(m => m.Selected && m.Name != ReferenceModFileName && m.Name != ModFileName).Select(m => m.Path).ToList();

                if (mods.Count == 0)
                {
                    throw new Exception("No mods selected to patch");
                }

                var installation = InstallationSelection.SelectedInstallation;

                var header = new Header(1, "LMayDev", "OpenConstructionSet compatibility patch to apply core values from SCAR's pathfinding fix to custom races");
                header.References.Add(ReferenceModFileName);
                header.Dependencies.AddRange(mods.Select(m => Path.GetFileName(m)));

                var options = new OcsDataContexOptions(ModName,
                                                       ThrowIfMissing: false,
                                                       Installation: installation,
                                                       Header: header,
                                                       BaseMods: mods,
                                                       LoadGameFiles: ModLoadType.Base);

                var context = builder.BuildAsync(options).GetAwaiter().GetResult();

                patcher.PatchAsync(installation, context).GetAwaiter().GetResult();

                context.SaveAsync().GetAwaiter().GetResult();

                var loadOrder = installationService.LoadEnabledModsAsync(installation)
                                                   .GetAwaiter()
                                                   .GetResult()
                                                   .ToList();

                loadOrder.RemoveAll(s => s.Equals(ModFileName, StringComparison.OrdinalIgnoreCase));

                loadOrder.Add(ModFileName);

                installationService.SaveEnabledModsAsync(installation, loadOrder)
                                   .GetAwaiter()
                                   .GetResult();

                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    "Mod created successfully",
                    "Mod created!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information));
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Failed to create mod:{Environment.NewLine}{ex.Message}",
                    "Error!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => Messenger<Refresh>.Send(new()));

                Busy = false;
            }
        }

        private void StartCreateMod()
        {
            Busy = true;

            Task.Run(CreateModExecute);
        }
    }
}