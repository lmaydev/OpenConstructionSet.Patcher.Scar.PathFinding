﻿using OpenConstructionSet.Installations;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    public class InstallationSelectionViewModel : BaseViewModel
    {
        private IInstallation[]? installations;
        private IInstallation? selectedInstallation;

        public InstallationSelectionViewModel(IInstallationService installationService)
        {
            LoadInstallations(installationService);
        }

        public IInstallation[]? Installations
        {
            get => installations;

            set
            {
                installations = value;

                OnPropertyChanged(nameof(Installations));
            }
        }

        public IInstallation? SelectedInstallation
        {
            get => selectedInstallation;

            set
            {
                selectedInstallation = value;

                OnPropertyChanged(nameof(SelectedInstallation));

                if (value is not null)
                {
                    Messenger<SelectedInstallationChanged>.Send(new(value));
                }
            }
        }

        private void LoadInstallations(IInstallationService installationService)
        {
            Installations = installationService.LocateAll().ToArray();

            if (Installations.Length == 0)
            {
                throw new Exception("Could not locate any installations");
            }

            SelectedInstallation = Installations[0];
        }
    }
}