using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    class LoadOrderViewModel
    {
        public RelayCommand RefreshMods { get; }

        public RelayCommand SelectAll { get; }

        public RelayCommand SelectNone { get; }

        public RelayCommand SaveLoadOrder { get; }

        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public ObservableCollection<GameFolder> Folders { get; }

        public LoadOrderViewModel(ObservableCollection<GameFolder> folders)
        {
            Folders = folders;

            RefreshMods = new RelayCommand(_ => RefreshExecute());

            SelectAll = new RelayCommand(_ => SelectAllExecute());
            SelectNone = new RelayCommand(_ => SelectNoneExecute());

            SaveLoadOrder = new RelayCommand(_ => SaveLoadOrderExecute());

            RefreshExecute();
        }

        private void SelectNoneExecute()
        {
            Mods.ForEach(m => m.Selected = false);
        }

        private void SelectAllExecute()
        {
            Mods.ForEach(m => m.Selected = true);
        }

        public void RefreshExecute()
        {
            Folders.ForEach(f => f.Populate());

            var mods = Folders.SelectMany(f => f.Mods)
                             // Not the reference mod, the new mod or a base file
                             .Where(p => !OcsHelper.BaseMods.Contains(p.Key))
                             // Group by mod name
                             .GroupBy(p => p.Key, p => p.Value)
                             // Convert to dictionary by taking first item from group
                             .ToDictionary(g => g.Key, g => g.First());


            //.Select(p => new ModViewModel { Name = p.Key, Path = p.Value });

            Mods.Clear();

            foreach (var loadOrderItem in LoadOrder.Read().Where(i => mods.ContainsKey(i)))
            {
                Mods.Add(new ModViewModel { Name = loadOrderItem, Path = mods[loadOrderItem], Selected = true });

                mods.Remove(loadOrderItem);
            }

            mods.ForEach(p => Mods.Add(new ModViewModel { Name = p.Key, Path = p.Value }));
        }

        private void SaveLoadOrderExecute()
        {
            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            LoadOrder.Save(mods);

            MessageBox.Show(Application.Current.MainWindow, "Load order saved", "Saved!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
