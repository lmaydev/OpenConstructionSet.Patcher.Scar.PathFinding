using forgotten_construction_set;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenConstructionSet;
using System.Windows;
using Microsoft.Win32;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        const string referenceName = "SCAR's pathfinding fix.mod";

        private string modName;
        private string folderList;
        bool executing;

        public string NewModPath
        {
            get => modName;
            set
            {
                modName = value;
                OnPropertyChanged(nameof(NewModPath));
            }
        }

        public string FolderList
        {
            get => folderList;
            set
            {
                folderList = value;
                OnPropertyChanged(nameof(FolderList));
            }
        }

        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public RelayCommand CreateMod { get; }

        public RelayCommand RefreshMods { get; }

        public RelayCommand BrowseNewMod { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            RefreshMods = new RelayCommand(_ => RefreshExecute());

            BrowseNewMod = new RelayCommand(_ => BrowseNewModExecute());

            if (OcsSteamHelper.TryFindGameFolders(out var folders))
            {
                var folderCollection = folders.ToArray();

                folderList = string.Join(Environment.NewLine, folderCollection.Select(f => f.FolderPath)) + Environment.NewLine;

                RefreshExecute();
            }
        }

        private bool CanCreateMod() => !executing && !string.IsNullOrEmpty(NewModPath) && Mods.Any(m => m.Selected);

        private void StartCreateMod()
        {
            executing = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = Mods.Where(m => m.Selected && m.Name != referenceName).ToList();

                if (mods.Count == 0)
                {
                    MessageBox.Show("No mods selected");
                    return;
                }

                var folders = ParseFolders();

                if (!folders.TryResolvePath(referenceName, out var referencePath))
                {
                    MessageBox.Show($"Could not find reference file ({referenceName})");
                    return;
                }

                var reference = OcsHelper.LoadSaveFile(referencePath);

                var modNames = new HashSet<string>(mods.Select(m => m.Name));

                var header = new GameData.Header
                {
                    Dependencies = modNames.ToList(),
                    Referenced = new List<string>(),
                    Version = reference.header.Version,
                    Description = BuildDescription(modNames),
                };

                header.Referenced.Add(referenceName);

                // HACK - NewMod doesn't accept a path so I split it
                OcsHelper.NewMod(header, Path.GetFileName(NewModPath), new GameFolder(Path.GetDirectoryName(NewModPath), GameFolderType.Data));

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewModPath, folders);

                var referenceRace = reference.items["17-gamedata.quack"];

                foreach (var race in data.items.OfType(itemType.RACE).Where(r => modNames.Contains(r.Mod)).Where(IsNotAnimal))
                {
                    race["pathfind acceleration"] = referenceRace["pathfind acceleration"];
                    race["water avoidance"] = referenceRace["water avoidance"];
                }

                data.save(NewModPath);

                MessageBox.Show($"Mod created successfully at {NewModPath}");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                executing = false;
            }

            bool IsNotAnimal(GameData.Item item)
            {
                var editorLimits = item["editor limits"] as GameData.File;

                return editorLimits != null && !string.IsNullOrEmpty(editorLimits.filename);
            }

            string AddMissingExtensions(string m) => string.IsNullOrEmpty(System.IO.Path.GetExtension(m)) ? $"{m}.mod" : m;

            string BuildDescription(IEnumerable<string> modNames)
            {
                var description = "Compatability patch for SCAR's pathfinding fix (https://www.nexusmods.com/kenshi/mods/602) and the following mods:\n";
                description += string.Join("\n", modNames.Select(System.IO.Path.GetFileNameWithoutExtension));
                description += "\nCreated automatically using the OpenConstructionKit (https://github.com/lmaydev/OpenConstructionSet)";

                return description;
            }
        }

        private void RefreshExecute()
        {
            var mods = ParseFolders().SelectMany(f => f.Mods)
                                      .Where(p => !OcsHelper.BaseMods.Contains(p.Key))
                                      .Select(p => new ModViewModel { Name = p.Key, Path = p.Value });

            Mods.Clear();

            foreach (var mod in mods)
            {
                Mods.Add(mod);
            }
        }

        private void BrowseNewModExecute()
        {
            var save = new SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = ".mod",
                Filter = "Mod File|*.mod",
                ValidateNames = true
            };

            if (save.ShowDialog() == true)
            {
                NewModPath = save.FileName;
            }
        }

        private IEnumerable<GameFolder> ParseFolders() => FolderList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                                           .Where(Directory.Exists)
                                                           .SelectMany(path => new[] { new GameFolder(path, GameFolderType.Data), new GameFolder(path, GameFolderType.Mod) });
    }
}
