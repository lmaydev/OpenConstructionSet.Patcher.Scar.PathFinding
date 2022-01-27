using OpenConstructionSet.Data;
using OpenConstructionSet.Models;
using OpenConstructionSet.Models.Enums;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ScarPathfindingFixPatcher
    {
        private readonly IOcsModService modService;

        public ScarPathfindingFixPatcher(IOcsModService modService)
        {
            this.modService = modService;
        }

        public async Task PatchAsync(InstallationInfo installation, OcsDataContext context)
        {
            var referenceMod = await modService.FindAsync(installation, "SCAR's pathfinding fix.mod") ??
                throw new Exception("Could not find SCAR's pathfinding fix.mod");

            var referenceData = await modService.ReadFileAsync(referenceMod.Path);

            context.Header.Version = referenceData.Header.Version;

            var greenlander = referenceData.Items.Find(i => i.Name.Equals("Greenlander", StringComparison.OrdinalIgnoreCase)) ??
                throw new Exception("Failed to load Greenlander item");

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

            static bool IsNotAnimal(DataItem race) => race.Values.TryGetValue("editor limits", out var value)
                                               && value is FileValue file
                                               && !string.IsNullOrEmpty(file.Path);
        }
    }
}