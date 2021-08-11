using forgotten_construction_set;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private bool busy;

        public string NewMod { get; set; }

        public bool Busy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        public FoldersViewModel Folders { get; }

        public LoadOrderViewModel LoadOrder { get; }
        
        public RelayCommand CreateMod { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            Folders = new FoldersViewModel();

            LoadOrder = new LoadOrderViewModel(Folders.Folders);

            Folders.Folders.CollectionChanged += (_, __) => LoadOrder.RefreshExecute();

            NewMod = "Compatibility SCAR's pathfinding fix";
        }

        private bool CanCreateMod() => !Busy && !string.IsNullOrWhiteSpace(NewMod) && LoadOrder.Mods.Any(m => m.Selected) && FileHelper.IsValidPath(NewMod);

        private void StartCreateMod()
        {
            Busy = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = LoadOrder.Mods.Where(m => m.Selected).ToList();

                Folders.Folders.ForEach(f => f.Populate());

                var modPath = CreateNewMod();

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewMod, Folders.Folders, resolveDependencies: false);

                var context = new PatchContext(data)
                {
                    Folders = Folders.Folders.ToArray(),
                    Mods = mods.ToDictionary(m => m.Name, m => m.Path),
                };

                new ScarPathfindingFixPatcher().Patch(context);

                data.save(modPath);

                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Mod created successfully",
                    "Mod created!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information));

                string CreateNewMod()
                {
                    var modNames = mods.Select(m => m.Name).ToList();

                    var header = new GameData.Header
                    {
                        Dependencies = modNames,
                        Referenced = new List<string>(),
                    };

                    return OcsHelper.NewMod(header, NewMod);
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Failed to create mod with error message:{ex.Message}",
                    "Error!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            finally
            {
                Busy = false;
            }
        }
    }
}
