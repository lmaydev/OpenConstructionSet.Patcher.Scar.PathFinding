using OpenConstructionSet.Data;
using OpenConstructionSet.Models;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private const string ModName = "OCSP - SCAR's pathfinding fix";
        private const string ModFileName = ModName + ".mod";
        private const string ReferenceModFileName = "SCAR's pathfinding fix.mod";
        private bool busy;
        private readonly IOcsDataContextBuilder builder;
        private readonly IOcsIOService io;
        private readonly ScarPathfindingFixPatcher patcher;

        public bool Busy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        public LoadOrderViewModel LoadOrder { get; }

        public RelayCommand CreateMod { get; }

        public InstallationSelectionViewModel InstallationSelection { get; }

        public MainViewModel(IOcsDataContextBuilder builder,
                             IOcsIOService io,
                             InstallationSelectionViewModel installationSelection,
                             LoadOrderViewModel loadOrder,
                             ScarPathfindingFixPatcher patcher)
        {
            this.builder = builder;
            this.io = io;
            InstallationSelection = installationSelection;

            LoadOrder = loadOrder;

            this.patcher = patcher;

            CreateMod = new RelayCommand(StartCreateMod, CanCreateMod);
        }

        private bool CanCreateMod() => !Busy && LoadOrder.Mods.Any(m => m.Selected);

        private void StartCreateMod()
        {
            Busy = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = LoadOrder.Mods.Where(m => m.Selected && m.Name != ReferenceModFileName && m.Name != ModFileName).Select(m => m.Path).ToList();

                if (!mods.Any())
                {
                    throw new Exception("No mods selected to patch");
                }

                var installation = InstallationSelection.SelectedInstallation.Installation;

                var header = new Header(1, "LMayDev", "OpenConstructionSet compatibility patch to apply core values from SCAR's pathfinding fix to custom races");
                header.References.Add(ReferenceModFileName);
                header.Dependencies.AddRange(mods.Select(m => Path.GetFileName(m)));

                var options = new OcsDataContexOptions(ModName,
                                                       ThrowIfMissing: false,
                                                       Installation: installation,
                                                       Header: header,
                                                       BaseMods: mods,
                                                       LoadGameFiles: Models.ModLoadType.Base);

                var context = builder.Build(options);

                patcher.Patch(installation, context);

                context.Save();

                if (!installation.EnabledMods.Contains(ModFileName))
                {
                    installation.EnabledMods.Add(ModFileName);

                    io.Write(installation.EnabledModsFile(), installation.EnabledMods);
                }

                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Mod created successfully",
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
    }
}
