using AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class DesignLoadOrderViewModel : LoadOrderViewModel
    {
        public DesignLoadOrderViewModel() : base(App.Fixture.Create<IOcsIOService>(),
                                                 App.Fixture.Create<InstallationSelectionViewModel>())
        {
        }
    }
}
