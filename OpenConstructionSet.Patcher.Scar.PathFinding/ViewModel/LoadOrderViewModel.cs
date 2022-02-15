using Microsoft.Toolkit.Mvvm.Input;
using OpenConstructionSet.Installations;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class LoadOrderViewModel : BaseViewModel
    {
        private IInstallation? installation;

        public LoadOrderViewModel()
        {
            Messenger<SelectedInstallationChanged>.MessageRecieved += InstallationChanged;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Messenger<Refresh>.MessageRecieved += _ => RefreshExecuteAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

        private void InstallationChanged(SelectedInstallationChanged message)
        {
            this.installation = message.SelectedInstallation;

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
            if (installation is null)
            {
                Mods.Clear();
                return;
            }

            // Get all mods except the base game files
            var mods = installation.GetMods().ExceptBy(OcsConstants.BaseMods, m => m.Filename).ToDictionary(m => m.Filename);

            var loadOrder = await installation.ReadEnabledModsAsync();

            Mods.Clear();

            // Add enabled mods first in load order and select
            foreach (var loadOrderItem in loadOrder.Where(m => mods.ContainsKey(m)))
            {
                var file = mods[loadOrderItem];

                var header = await file.ReadHeaderAsync();

                Mods.Add(new ModViewModel(file.Filename, file.Path, header, true));
                mods.Remove(loadOrderItem);
            }

            // Add remaining mods un-selected
            foreach (var file in mods.Values)
            {
                var header = await file.ReadHeaderAsync();

                Mods.Add(new ModViewModel(file.Filename, file.Path, header, false));
            }
        }

        private async Task SaveLoadOrderExecuteAsync()
        {
            if (installation is null)
            {
                return;
            }

            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            await installation.WriteEnabledModsAsync(mods);

            Messenger<Infrastructure.Messages.ShowMessageBox>.Send(new("Load order saved", "Saved!", MessageBoxImage.Information));
        }

        private void SelectExecute(bool select)
        {
            Mods.ForEach(m => m.Selected = select);
        }
    }
}