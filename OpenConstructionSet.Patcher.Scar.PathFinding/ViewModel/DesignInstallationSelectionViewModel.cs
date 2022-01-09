using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class DesignInstallationSelectionViewModel : InstallationSelectionViewModel
    {
        public DesignInstallationSelectionViewModel() : base(App.Fixture.Create<IOcsDiscoveryService>())
        {
        }
    }
}
