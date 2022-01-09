using OpenConstructionSet.Models;
using System.IO;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure
{
    internal static class OcsExtensions
    {
        public static string EnabledModsFile(this Installation installation) => Path.Combine(installation.Data.FullName, OcsConstants.EnabledModFile);
    }
}
