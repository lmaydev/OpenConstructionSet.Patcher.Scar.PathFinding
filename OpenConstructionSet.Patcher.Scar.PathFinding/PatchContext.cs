using forgotten_construction_set;
using System.Collections.Generic;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    public class PatchContext
    {
        public GameData GameData { get; }

        public GameFolder[] Folders { get; set; }

        public IDictionary<string, string> Mods { get; set; }

        public PatchContext(GameData gameData)
        {
            GameData = gameData;
        }
    }
}