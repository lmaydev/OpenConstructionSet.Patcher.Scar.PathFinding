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

                if (!Folders.TryResolvePath(referenceName, out var referencePath))
                {
                    MessageBox.Show($"Could not find reference file ({referenceName})");
                    return;
                }

                var reference = OcsHelper.LoadSaveFile(referencePath);

                var modNames = new HashSet<string>(mods.Select(m => m.Name));

                var modPath = CreateNewMod();

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewMod, Folders);

                var referenceRace = reference.items["17-gamedata.quack"];

                var pathFindAcceleration = referenceRace["pathfind acceleration"];
                var waterAvoidence = referenceRace["water avoidance"];

                foreach (var race in data.items.OfType(itemType.RACE).Where(r => modNames.Contains(r.Mod) && IsNotAnimal(r)))
                {
                    race["pathfind acceleration"] = pathFindAcceleration;
                    race["water avoidance"] = waterAvoidence;
                }

                data.save(modPath);

                MessageBox.Show($"Mod created successfully at {modPath}");

                string CreateNewMod()
                {
                    var header = new GameData.Header
                    {
                        Dependencies = modNames.ToList(),
                        Referenced = new List<string>(),
                        Version = reference.header.Version,
                        Description = BuildDescription(),
                    };

                    header.Referenced.Add(referenceName);

                    return OcsHelper.NewMod(header, NewMod);
                }

                string BuildDescription()
                {
                    var builder = new StringBuilder();


                    builder.AppendLine("Compatability patch for SCAR's pathfinding fix (https://www.nexusmods.com/kenshi/mods/602) and the following mods:");
                    foreach (var modName in modNames.Select(Path.GetFileNameWithoutExtension))
                    {
                        builder.AppendLine(modName);
                    }

                    builder.AppendLine();
                    builder.AppendLine("Created automatically using the OpenConstructionKit (https://github.com/lmaydev/OpenConstructionSet)");
                    builder.AppendLine("Source code for the patcher is available at https://github.com/lmaydev/OpenConstructionSet.Patcher.Scar.PathFinding");

                    return builder.ToString();
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

            // HACK - not keen on this method of discovery
            bool IsNotAnimal(GameData.Item item)
            {
                var editorLimits = item["editor limits"] as GameData.File;

                return editorLimits != null && !string.IsNullOrEmpty(editorLimits.filename);
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
