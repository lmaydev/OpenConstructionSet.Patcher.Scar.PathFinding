using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;
using System;
using System.Linq;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    public class InstallationSelectionViewModel : BaseViewModel
    {
        private InstallationViewModel selectedInstallation;

        private InstallationViewModel[] installations;
        private readonly IOcsDiscoveryService discovery;

        public InstallationViewModel[] Installations
        {
            get => installations;

            set
            {
                installations = value;

                OnPropertyChanged(nameof(Installations));
            }
        }

        public InstallationViewModel SelectedInstallation
        {
            get => selectedInstallation;

            set
            {
                selectedInstallation = value;

                OnPropertyChanged(nameof(SelectedInstallation));

                if (value is not null)
                {
                    Messenger<InstallationViewModel>.Send(value);
                }
            }
        }

        public InstallationSelectionViewModel(IOcsDiscoveryService discovery)
        {
            installations = discovery.DiscoverAllInstallations().Select(p => new InstallationViewModel(p)).ToArray();

            if (Installations.Length == 0)
            {
                throw new Exception("Could not locate any installations");
            }

            selectedInstallation = Installations[0];

            Messenger<Refresh>.MessageRecieved += MessageRecieved;
            this.discovery = discovery;
        }

        private void MessageRecieved(Refresh obj)
        {
            var currentName = selectedInstallation.Name;

            Installations = discovery.DiscoverAllInstallations().Select(p => new InstallationViewModel(p)).ToArray();

            if (Installations.Length == 0)
            {
                throw new Exception("Could not locate any installations");
            }

            SelectedInstallation = Installations.FirstOrDefault(i => i.Name == currentName) ?? Installations[0];
        }
    }
}