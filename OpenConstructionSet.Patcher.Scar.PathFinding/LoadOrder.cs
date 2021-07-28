using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    public static class LoadOrder
    {
        private static readonly string configFile = Path.Combine(OcsHelper.LocalFolders.Data.FolderPath, "mods.cfg");

        public static string[] Read() => File.Exists(configFile) ? File.ReadAllLines(configFile) : Array.Empty<string>();

        public static void Save(string[] mods)
        {
            File.Delete(configFile);
            File.WriteAllLines(configFile, mods);
        }
    }
}
