using OpenConstructionSet.Installations;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Infrastructure.Messages
{
    internal record struct SelectedInstallationChanged(IInstallation SelectedInstallation)
    {
    }
}