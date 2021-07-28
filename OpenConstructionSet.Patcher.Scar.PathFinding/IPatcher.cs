using forgotten_construction_set;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    interface IPatcher
    {
        string Name { get; }

        void Patch(PatchContext context);
    }

    class ScarPathfindingFixPatcher : IPatcher
    {
        const string referenceName = "SCAR's pathfinding fix.mod";

        public string Name { get; set; } = "OCS Patcher - Scar's pathfinding fix";

        public void Patch(PatchContext context)
        {
            if (!context.Folders.TryResolvePath(referenceName, out var referencePath))
            {
                throw new Exception($"Could not find reference file ({referenceName})");
            }

            var reference = OcsHelper.LoadSaveFile(referencePath);

            context.GameData.header.Version = reference.header.Version;
            context.GameData.header.Referenced.Add(referenceName);
            context.GameData.header.Description = BuildDescription();

            var referenceRace = reference.items["17-gamedata.quack"];

            var pathFindAcceleration = referenceRace["pathfind acceleration"];
            var waterAvoidence = referenceRace["water avoidance"];

            foreach (var race in context.GameData.items.OfType(itemType.RACE).Where(r => context.Mods.Keys.Contains(r.Mod) && IsNotAnimal(r) && IsNotDeleted(r)))
            {
                race["pathfind acceleration"] = pathFindAcceleration;
                race["water avoidance"] = waterAvoidence;
            }

            bool IsNotDeleted(GameData.Item item)
            {
                var state = item.getState();

                return state != GameData.State.REMOVED && state != GameData.State.LOCKED_REMOVED;
            }

            // HACK - not keen on this method of discovery
            bool IsNotAnimal(GameData.Item item)
            {
                var editorLimits = item["editor limits"] as GameData.File;

                return editorLimits != null && !string.IsNullOrEmpty(editorLimits.filename);
            }

            string BuildDescription()
            {
                var builder = new StringBuilder();


                builder.AppendLine("Compatability patch for SCAR's pathfinding fix (https://www.nexusmods.com/kenshi/mods/602) and the following mods:");
                foreach (var modName in context.GameData.header.Referenced.Select(System.IO.Path.GetFileNameWithoutExtension))
                {
                    builder.AppendLine(modName);
                }

                builder.AppendLine();
                builder.AppendLine("Created automatically using the OpenConstructionKit (https://github.com/lmaydev/OpenConstructionSet)");
                builder.AppendLine("Source code for the patcher is available at https://github.com/lmaydev/OpenConstructionSet.Patcher.Scar.PathFinding");

                return builder.ToString();
            }
        }
    }
}
