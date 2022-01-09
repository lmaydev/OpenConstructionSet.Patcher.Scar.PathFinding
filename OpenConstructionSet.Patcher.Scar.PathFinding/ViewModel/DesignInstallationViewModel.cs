using AutoFixture;
using OpenConstructionSet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal record DesignInstallationViewModel : InstallationViewModel
    {
        public DesignInstallationViewModel() : base(App.Fixture.Create<string>(), App.Fixture.Create<Installation>())
        {
        }
    }
}
