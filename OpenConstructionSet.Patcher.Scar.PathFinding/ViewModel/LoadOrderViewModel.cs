using Microsoft.Toolkit.Mvvm.Input;
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

        private readonly IOcsModService modService;

        private InstallationInfo installation;
        private int selectedModIndex;

        public LoadOrderViewModel(IOcsModService modService, IOcsInstallationService installationService, InstallationSelectionViewModel installationsViewModel)
        {
            Messenger<InstallationInfo>.MessageRecieved += InstallationChanged;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Messenger<Refresh>.MessageRecieved += _ => RefreshExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            installation = installationsViewModel.SelectedInstallation;

            this.modService = modService;
            this.installationService = installationService;

            Refresh = new AsyncRelayCommand(RefreshExecuteAsync);
            SaveLoadOrder = new AsyncRelayCommand(SaveLoadOrderExecuteAsync);

            Select = new RelayCommand<bool>(SelectExecute);

            MoveUp = new RelayCommand<ModViewModel>(MoveUpExecute);

            MoveDown = new RelayCommand<ModViewModel>(MoveDownExecute);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();
        public RelayCommand<ModViewModel> MoveDown { get; }
        public RelayCommand<ModViewModel> MoveUp { get; }
        public AsyncRelayCommand Refresh { get; }
        public AsyncRelayCommand SaveLoadOrder { get; }
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RefreshExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

        private async Task RefreshExecuteAsync()
        {
            var mods = await modService.FindAllAsync(installation).ToDictionaryAsync(m => m.Filename);

            var loadOrder = await installationService.LoadEnabledModsAsync(installation);

            Mods.Clear();

            foreach (var loadOrderItem in loadOrder.Where(i => mods.ContainsKey(i)))
            {
                Mods.Add(new ModViewModel(loadOrderItem, mods[loadOrderItem].Path, true));
                mods.Remove(loadOrderItem);
            }

            mods.ForEach(p => Mods.Add(new ModViewModel(p.Key, p.Value.Path, false)));
        }

        private async Task SaveLoadOrderExecuteAsync()
        {
            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            await installationService.SaveEnabledModsAsync(installation, mods);

            Messenger<Infrastructure.Messages.MessageBox>.Send(new("Load order saved", "Saved!", MessageBoxImage.Information));
        }

        private void SelectExecute(bool select)
        {
            Mods.ForEach(m => m.Selected = select);
        }
    }
}