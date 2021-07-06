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

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private string referenceModPath;
        private string modList;
        private string modName;

        public string ReferenceModPath
        {
            get => referenceModPath;
            set
            {
                referenceModPath = value;
                OnPropertyChanged(nameof(ReferenceModPath));
            }
        }

        public string ModName
        {
            get => modName;
            set
            {
                modName = value;
                OnPropertyChanged(nameof(ModName));
            }
        }

        public string ModList
        {
            get => modList;
            set
            {
                modList = value;
                OnPropertyChanged(nameof(ModList));
            }
        }

        bool executing;

        private string[] ModsToPatch => string.IsNullOrEmpty(ModList) ? Array.Empty<string>() : modList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        public RelayCommand CreateMod { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            if (OcsSteamHelper.TryFindGameFolders(out var folders) && folders.ToArray().TryResolvePath("SCAR's pathfinding fix.mod", out var path))
            {
                referenceModPath = path;
            }
        }

        private bool CanCreateMod() => !executing && !string.IsNullOrEmpty(ModName) && !string.IsNullOrEmpty(ModList) && File.Exists(referenceModPath);

        private void StartCreateMod()
        {
            executing = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            var reference = OcsHelper.LoadSaveFile(referenceModPath);

            var mods = ModsToPatch;

            var dependencies = mods.Select(Path.GetFileName).Distinct().ToList();

            var description = "Compatability patch for SCAR's pathfinding fix (https://www.nexusmods.com/kenshi/mods/602) and the following mods:\n";
            description += string.Join("\n", dependencies.Select(Path.GetFileNameWithoutExtension));
            description += "\nCreated automatically using the OpenConstructionKit (https://github.com/lmaydev/OpenConstructionSet)";

            var header = new GameData.Header
            {
                Dependencies = dependencies,
                Referenced = (new[] { Path.GetFileName(referenceModPath) }).ToList(),
                Version = reference.header.Version,
                Description = description,
            };

            if (!ModName.EndsWith(".mod"))
            {
                ModName = $"{ModName}.mod";
            }

            var path = OcsHelper.NewMod(header, ModName);

            var data = OcsHelper.Load(mods, path);

            var referenceRace = reference.items["17-gamedata.quack"];

            foreach (var race in data.items.OfType(itemType.RACE).Where(r => dependencies.Contains(r.Mod)).Where(IsNotAnimal))
            {
                race["pathfind acceleration"] = referenceRace["pathfind acceleration"];
                race["water avoidance"] = referenceRace["water avoidance"];
            }

            data.save(path);

            System.Windows.MessageBox.Show($"Mod created successfully at {path}");

            executing = false;

            bool IsNotAnimal(GameData.Item item)
            {
                var editorLimits = item["editor limits"] as GameData.File;

                return editorLimits != null && !string.IsNullOrEmpty(editorLimits.filename);
            }
        }
    }
}
