using OpenConstructionSet.Models;
using OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    public class InstallationSelectionViewModel : BaseViewModel
    {
        private InstallationInfo[] installations;
        private InstallationInfo selectedInstallation;

        public InstallationSelectionViewModel(IOcsInstallationService installationService)
        {
            installations = installationService.LocateAllAsync()
                                               .ToArrayAsync()
                                               .AsTask()
                                               .GetAwaiter()
                                               .GetResult();

            if (Installations.Length == 0)
            {
                throw new Exception("Could not locate any installations");
            }

            selectedInstallation = Installations[0];
        }

        public InstallationInfo[] Installations
        {
            get => installations;

            set
            {
                installations = value;

                OnPropertyChanged(nameof(Installations));
            }
        }

        public InstallationInfo SelectedInstallation
        {
            get => selectedInstallation;

            set
            {
                selectedInstallation = value;

                OnPropertyChanged(nameof(SelectedInstallation));

                if (value is not null)
                {
                    Messenger<InstallationInfo>.Send(value);
                }
            }
        }
    }
}