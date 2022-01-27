using OpenConstructionSet.Models;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class LoadOrderViewModel : BaseViewModel
    {
        private readonly IOcsInstallationService installationService;

        private readonly IOcsModService modService
                    ;

        private InstallationInfo installation;
        private int selectedModIndex;

        public LoadOrderViewModel(IOcsModService modService, IOcsInstallationService installationService, InstallationSelectionViewModel installationsViewModel)
        {
            Messenger<InstallationInfo>.MessageRecieved += InstallationChanged;
            Messenger<Refresh>.MessageRecieved += _ => RefreshExecute();

            installation = installationsViewModel.SelectedInstallation;

            this.modService = modService;
            this.installationService = installationService;

            Refresh = new RelayCommand(RefreshExecute);

            Select = new RelayCommand<bool>(SelectExecute);

            SaveLoadOrder = new RelayCommand(SaveLoadOrderExecute);

            MoveUp = new RelayCommand<ModViewModel>(MoveUpExecute);

            MoveDown = new RelayCommand<ModViewModel>(MoveDownExecute);

            RefreshMods();
        }

        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();
        public RelayCommand<ModViewModel> MoveDown { get; }
        public RelayCommand<ModViewModel> MoveUp { get; }
        public RelayCommand Refresh { get; }

        public RelayCommand SaveLoadOrder { get; }
        public RelayCommand<bool> Select { get; }

        public int SelectedModIndex
        {
            get => selectedModIndex;

            set
            {
                selectedModIndex = value;
                OnPropertyChanged(nameof(SelectedModIndex));
            }
        }

        private void InstallationChanged(InstallationInfo installation)
        {
            this.installation = installation;

            RefreshMods();
        }

        private void MoveDownExecute(ModViewModel? mod)
        {
            if (mod is null)
            {
                return;
            }

            var index = Mods.IndexOf(mod);

            var newIndex = index < Mods.Count - 1 ? index + 1 : 0;

            Mods.Move(index, newIndex);
        }

        private void MoveUpExecute(ModViewModel? mod)
        {
            if (mod is null)
            {
                return;
            }

            var index = Mods.IndexOf(mod);

            var newIndex = Mods.IndexOf(mod) > 0 ? index - 1 : Mods.Count - 1;

            Mods.Move(index, newIndex);
        }

        private void RefreshExecute()
        {
            RefreshMods();
        }

        private void RefreshMods()
        {
            var mods = modService.FindAllAsync(installation)
                                 .ToArrayAsync()
                                 .AsTask()
                                 .GetAwaiter()
                                 .GetResult()
                                 .ToDictionary(m => m.Filename);

            var loadOrder = installationService.LoadEnabledModsAsync(installation).GetAwaiter().GetResult();

            Mods.Clear();

            foreach (var loadOrderItem in loadOrder.Where(i => mods.ContainsKey(i)))
            {
                Mods.Add(new ModViewModel(loadOrderItem, mods[loadOrderItem].Path, true));
                mods.Remove(loadOrderItem);
            }

            mods.ForEach(p => Mods.Add(new ModViewModel(p.Key, p.Value.Path, false)));
        }

        private void SaveLoadOrderExecute()
        {
            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            installationService.SaveEnabledModsAsync(installation, mods).GetAwaiter().GetResult();

            MessageBox.Show(Application.Current.MainWindow, "Load order saved", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectExecute(bool select)
        {
            Mods.ForEach(m => m.Selected = select);
        }
    }
}