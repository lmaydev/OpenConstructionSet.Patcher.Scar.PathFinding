using OpenConstructionSet.Data;
using OpenConstructionSet.Installations;
using OpenConstructionSet.Mods;
using OpenConstructionSet.Mods.Context;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ScarPathfindingFixPatcher
    {
        public async Task PatchAsync(IInstallation installation, IModContext context)
        {
            if (!installation.TryFind("SCAR's pathfinding fix.mod", out var referenceFile))
            {
                throw new Exception("Could not find SCAR's pathfinding fix.mod");
            }

            var referenceData = await referenceFile.ReadDataAsync();

            context.Header.Version = referenceData.Header.Version;

            var greenlander = referenceData.Items.Find(i => i.Name == "Greenlander") ?? throw new Exception("Failed to load Greenlander item");

            var pathfindAcceleration = greenlander.Values["pathfind acceleration"];
            var waterAvoidance = greenlander.Values["water avoidance"];

            foreach (var race in context.Items.OfType(ItemType.Race).Where(IsNotAnimal))
            {
                Console.WriteLine("Updating " + race.Name);
                race.Values["pathfind acceleration"] = pathfindAcceleration;

                // avoid changing for races that like water
                if ((float)race.Values["water avoidance"] > 0)
                {
                    race.Values["water avoidance"] = waterAvoidance;
                }
            }

            static bool IsNotAnimal(ModItem race) => race.Values.TryGetValue("editor limits", out var value)
                                               && value is FileValue file
                                               && !string.IsNullOrEmpty(file.Path);
        }
    }
}