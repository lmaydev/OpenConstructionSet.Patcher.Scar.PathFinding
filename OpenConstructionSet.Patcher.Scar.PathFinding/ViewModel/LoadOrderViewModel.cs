using OpenConstructionSet.Models;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class LoadOrderViewModel : BaseViewModel
    {
        private InstallationViewModel installation;
        private int selectedModIndex;
        private readonly IOcsIOService io;

        public RelayCommand Refresh { get; }

        public RelayCommand<bool> Select { get; }

        public RelayCommand SaveLoadOrder { get; }

        public RelayCommand<ModViewModel> MoveUp { get; }

        public RelayCommand<ModViewModel> MoveDown { get; }

        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public int SelectedModIndex
        {
            get => selectedModIndex;

            set
            {
                selectedModIndex = value;
                OnPropertyChanged(nameof(SelectedModIndex));
            }
        }

        public LoadOrderViewModel(IOcsIOService io, InstallationSelectionViewModel installationsViewModel)
        {
            Messenger<InstallationViewModel>.MessageRecieved += InstallationChanged;

            installation = installationsViewModel.SelectedInstallation;

            this.io = io;

            Refresh = new RelayCommand(RefreshExecute);

            Select = new RelayCommand<bool>(SelectExecute);

            SaveLoadOrder = new RelayCommand(SaveLoadOrderExecute);

            MoveUp = new RelayCommand<ModViewModel>(MoveUpExecute);

            MoveDown = new RelayCommand<ModViewModel>(MoveDownExecute);

            RefreshMods();
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

        private void InstallationChanged(InstallationViewModel installation)
        {
            this.installation = installation;

            RefreshMods();
        }

        private void SelectExecute(bool select)
        {
            Mods.ForEach(m => m.Selected = select);
        }

        private void RefreshExecute()
        {
            Messenger<Refresh>.Send(new());

            RefreshMods();
        }

        private void RefreshMods()
        {
            var folders = new List<ModFolder>
            {
                installation.Installation.Mod
            };

            if (installation.Installation.Content is not null)
            {
                folders.Add(installation.Installation.Content);
            }

            var mods = new Dictionary<string, ModFile>(folders.SelectMany(f => f.Mods)
                                                              .Where(p => !OcsConstants.BaseMods.Contains(p.Key))
                                                              .DistinctBy(p => p.Key));

            Mods.Clear();

            foreach (var loadOrderItem in installation.Installation.EnabledMods.Where(i => mods.ContainsKey(i)))
            {
                Mods.Add(new ModViewModel(loadOrderItem, mods[loadOrderItem].FullName, true));
                mods.Remove(loadOrderItem);
            }

            mods.ForEach(p => Mods.Add(new ModViewModel(p.Key, p.Value.FullName, false)));
        }

        private void SaveLoadOrderExecute()
        {
            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            io.Write(installation.Installation.EnabledModsFile(), mods);

            MessageBox.Show(Application.Current.MainWindow, "Load order saved", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
