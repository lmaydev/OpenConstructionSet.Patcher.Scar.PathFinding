using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class FoldersViewModel
    {
        public ObservableCollection<GameFolder> Folders { get; } = new ObservableCollection<GameFolder>();

        public GameFolder CurrentFolder { get; set; }

        public RelayCommand AddFolder { get; }

        public RelayCommand RemoveFolder { get; }

        public FoldersViewModel()
        {
            AddFolder = new RelayCommand(_ => AddFolderExecute());

            RemoveFolder = new RelayCommand(_ => RemoveFolderExecute(), _ => CanRemoveFolder());

            Folders.Add(OcsHelper.LocalFolders.Data);
            Folders.Add(OcsHelper.LocalFolders.Mod);

#if STEAM
            try
            {
                if (OcsSteamHelper.TryFindContentFolder(out var contentFolder))
                {
                    Folders.Add(contentFolder);
                }
            }
            catch
            {
            } 
#endif
        }

        private void AddFolderExecute()
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a new mod folder to use",
                ShowNewFolderButton = true,
            };

            var owner = Application.Current.MainWindow.AsWin32();

            if (folder.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
            {
                Folders.Add(new GameFolder(folder.SelectedPath));
            }
        }

        private bool CanRemoveFolder() => CurrentFolder != null;

        private void RemoveFolderExecute()
        {
            Folders.Remove(CurrentFolder);
        }

    }
}
