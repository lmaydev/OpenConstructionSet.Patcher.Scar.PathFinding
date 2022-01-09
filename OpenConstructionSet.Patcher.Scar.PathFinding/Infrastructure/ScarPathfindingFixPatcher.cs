using OpenConstructionSet.Data;
using OpenConstructionSet.Data.Models;
using OpenConstructionSet.Models;
using System;
using System.Linq;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ScarPathfindingFixPatcher
    {
        private readonly IOcsIOService io;

        public ScarPathfindingFixPatcher(IOcsIOService io)
        {
            this.io = io;
        }

        public void Patch(Installation installation, OcsDataContext context)
        {
            if (!installation.Mod.Mods.TryGetValue("SCAR's pathfinding fix.mod", out var referneceMod))
            {
                throw new Exception("Could not find SCAR's pathfinding fix.mod");
            }

            var referenceData = io.ReadDataFile(referneceMod.FullName) ?? throw new Exception("Failed to read SCAR's pathfinding fix.mod");

            context.Header.Version = referenceData.Header!.Version;

            var greenlander = referenceData.Items.Find(i => i.StringId == "17-gamedata.quack")!;
            var pathfindAcceleration = greenlander.Values["pathfind acceleration"];
            var waterAvoidance = greenlander.Values["water avoidance"];

            foreach (var race in context.Items.Values.OfType(ItemType.Race).Where(IsNotAnimal))
            {
                Console.WriteLine("Updating " + race.Name);
                race.Values["pathfind acceleration"] = pathfindAcceleration;

                // avoid changing for races that like water
                if ((float)race.Values["water avoidance"] > 0)
                {
                    race.Values["water avoidance"] = waterAvoidance;
                }
            }

            bool IsNotAnimal(DataItem race) => race.Values.TryGetValue("editor limits", out var value)
                                               && value is FileValue file
                                               && !string.IsNullOrEmpty(file.Path);
        }
    }
}