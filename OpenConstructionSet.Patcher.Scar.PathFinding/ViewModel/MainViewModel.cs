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

        private IEnumerable<string> previousFolders;

        private string modName;
        private string folderList;

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

                if (!SplitFolders().OrderBy(f => f).SequenceEqual(previousFolders))
                {
                    RefreshExecute();
                }

                OnPropertyChanged(nameof(FolderList));
            }
        }

        private bool processing;
        private bool refreshing;

        public bool Processing
        {
            get { return processing; }
            set
            {
                processing = value;
                OnPropertyChanged(nameof(Processing));
            }
        }

        public bool Refreshing
        {
            get { return refreshing; }
            set
            {
                refreshing = value;
                OnPropertyChanged(nameof(Processing));
            }
        }


        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public RelayCommand CreateMod { get; }

        public RelayCommand RefreshMods { get; }

        public RelayCommand BrowseNewMod { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            RefreshMods = new RelayCommand(_ => RefreshExecute(), _ => IsBusy());

            BrowseNewMod = new RelayCommand(_ => BrowseNewModExecute(), _ => IsBusy());

            NewModPath = @"mods\new\new.mod";

            var folders = new HashSet<string>();

            if (OcsSteamHelper.TryFindGameFolders(out var gameFolders))
            {
                folders.Add(gameFolders.Data.FolderPath);
                folders.Add(gameFolders.Mod.FolderPath);
            }

            folders.Add(Path.Combine(Environment.CurrentDirectory, "mods"));
            folders.Add(Path.Combine(Environment.CurrentDirectory, "data"));

            folderList = string.Join(Environment.NewLine, folders.Where(Directory.Exists)) + Environment.NewLine;

            RefreshExecute();
        }

        private bool CanCreateMod() => !Processing && !Refreshing && !string.IsNullOrEmpty(NewModPath) && Mods.Any(m => m.Selected) && FileHelper.IsValidPath(NewModPath);

        private bool IsBusy() => !Refreshing && !Processing;

        private void StartCreateMod()
        {
            Processing = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = Mods.Where(m => m.Selected).ToList();

                var folders = ParseFolders(previousFolders);

                if (!folders.TryResolvePath(referenceName, out var referencePath))
                {
                    MessageBox.Show($"Could not find reference file ({referenceName})");
                    return;
                }

                var reference = OcsHelper.LoadSaveFile(referencePath);

                var modNames = new HashSet<string>(mods.Select(m => m.Name));

                if (!FileHelper.TryGetFullPath(NewModPath, out var fullNewModPath))
                {
                    throw new Exception($"Failed to get full path ({NewModPath})");
                }

                CreateNewMod();

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewModPath, folders);

                var referenceRace = reference.items["17-gamedata.quack"];

                var pathFindAcceleration = referenceRace["pathfind acceleration"];
                var waterAvoidence = referenceRace["water avoidance"];

                foreach (var race in data.items.OfType(itemType.RACE).Where(r => modNames.Contains(r.Mod) && IsNotAnimal(r)))
                {
                    race["pathfind acceleration"] = pathFindAcceleration;
                    race["water avoidance"] = waterAvoidence;
                }

                data.save(fullNewModPath);

                MessageBox.Show($"Mod created successfully at {fullNewModPath}");

                void CreateNewMod()
                {
                    var newModDirectory = Path.GetDirectoryName(fullNewModPath);
                    var newModName = Path.GetFileName(fullNewModPath);

                    if (!Directory.Exists(newModDirectory))
                    {
                        Directory.CreateDirectory(newModDirectory);
                    }

                    var header = new GameData.Header
                    {
                        Dependencies = modNames.ToList(),
                        Referenced = new List<string>(),
                        Version = reference.header.Version,
                        Description = BuildDescription(),
                    };

                    header.Referenced.Add(referenceName);

                    // HACK - NewMod doesn't accept a path so I split it
                    OcsHelper.NewMod(header, newModName, new GameFolder(newModDirectory, GameFolderType.Data));
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
                Processing = false;
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
            previousFolders = SplitFolders().OrderBy(f => f).ToArray();

            var mods = ParseFolders(previousFolders).SelectMany(f => f.Mods)
                                                    .Where(p => p.Key != referenceName && p.Value != NewModPath && !OcsHelper.BaseMods.Contains(p.Key))
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

        private IEnumerable<string> SplitFolders() => FolderList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(Directory.Exists);

        private IEnumerable<GameFolder> ParseFolders(IEnumerable<string> paths) => paths.SelectMany(path => new[] 
                                                                                        { 
                                                                                            // HACK - no support for dual types
                                                                                            new GameFolder(path, GameFolderType.Data), 
                                                                                            new GameFolder(path, GameFolderType.Mod) 
                                                                                        });
    }
}
