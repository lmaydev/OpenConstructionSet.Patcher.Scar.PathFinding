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
        const string referenceName = "SCAR's pathfinding fix.mod";

        private bool busy;

        public ObservableCollection<GameFolder> Folders { get; } = new ObservableCollection<GameFolder>();

        public GameFolder CurrentFolder { get; set; }

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


        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public RelayCommand CreateMod { get; }

        public RelayCommand RefreshMods { get; }

        public RelayCommand AddFolder { get; }

        public RelayCommand RemoveFolder { get; }

        public RelayCommand SelectAll { get; }

        public RelayCommand SelectNone { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            RefreshMods = new RelayCommand(_ => RefreshExecute(), _ => IsNotBusy());

            AddFolder = new RelayCommand(_ => AddFolderExecute(), _ => IsNotBusy());

            RemoveFolder = new RelayCommand(_ => RemoveFolderExecute(), _ => CanRemoveFolder());

            SelectAll = new RelayCommand(_ => SelectAllExecute(), _ => IsNotBusy());
            SelectNone = new RelayCommand(_ => SelectNoneExecute(), _ => IsNotBusy());

            NewMod = "Compatibility SCAR's pathfinding fix";

            Folders.Add(OcsHelper.LocalFolders.Data);
            Folders.Add(OcsHelper.LocalFolders.Mod);

            Folders.CollectionChanged += (_, __) => RefreshExecute();

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

        private bool CanRemoveFolder() => CurrentFolder != null && IsNotBusy();

        private void RemoveFolderExecute()
        {
            Folders.Remove(CurrentFolder);
        }

        private bool CanCreateMod() => !Busy && !string.IsNullOrEmpty(NewMod) && Mods.Any(m => m.Selected) && FileHelper.IsValidPath(NewMod);

        private bool IsNotBusy() => !Busy;

        private void AddFolderExecute()
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a new mod folder to use",
                ShowNewFolderButton = true,
            };

            var owner = Application.Current.MainWindow.AsWin32();

            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folders.Add(new GameFolder(folder.SelectedPath, GameFolderType.Both));
            }
        }

        private void StartCreateMod()
        {
            Busy = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = Mods.Where(m => m.Selected).ToList();

                Folders.ForEach(f => f.Populate());

                var modNames = new HashSet<string>(mods.Select(m => m.Name));

                var modPath = CreateNewMod();

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewMod, Folders);

                var context = new PatchContext(data)
                {
                    Folders = Folders.ToArray(),
                    Mods = mods.ToDictionary(m => m.Name, m => m.Path),
                };

                data.save(modPath);

                MessageBox.Show($"Mod created successfully at {modPath}");

                string CreateNewMod()
                {
                    var header = new GameData.Header
                    {
                        Dependencies = modNames.ToList(),
                        Referenced = new List<string>(),
                    };

                    return OcsHelper.NewMod(header, NewMod);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create mod with error message:{ex.Message}");
            }
            finally
            {
                Busy = false;
            }
        }

        private void RefreshExecute()
        {
            Folders.ForEach(f => f.Populate());

            var mods = Folders.SelectMany(f => f.Mods)
                              // Not the reference mod, the new mod or a base file
                              .Where(p => p.Key != referenceName && !p.Key.StartsWith(NewMod) && !OcsHelper.BaseMods.Contains(p.Key))
                              .Select(p => new ModViewModel { Name = p.Key, Path = p.Value });

            Mods.Clear();

            mods.ForEach(m => Mods.Add(m));
        }
    }
}
