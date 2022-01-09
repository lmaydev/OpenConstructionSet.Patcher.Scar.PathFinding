using OpenConstructionSet.Models;
using System.Collections.Generic;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    public record InstallationViewModel(string Name, Installation Installation)
    {
        public InstallationViewModel(KeyValuePair<string, Installation> pair) : this(pair.Key, pair.Value)
        {
        }
    }
}