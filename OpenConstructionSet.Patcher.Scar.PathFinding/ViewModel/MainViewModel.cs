﻿using forgotten_construction_set;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        private bool busy;

        public ObservableCollection<GameFolder> Folders { get; } = new ObservableCollection<GameFolder>();

        public GameFolder CurrentFolder { get; set; }

        public string NewMod { get; set; }

        public bool Busy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }


        public ObservableCollection<ModViewModel> Mods { get; } = new ObservableCollection<ModViewModel>();

        public RelayCommand CreateMod { get; }

        public RelayCommand RefreshMods { get; }

        public RelayCommand AddFolder { get; }

        public RelayCommand RemoveFolder { get; }

        public RelayCommand SelectAll { get; }

        public RelayCommand SelectNone { get; }

        public RelayCommand SaveLoadOrder { get; }

        public MainViewModel()
        {
            CreateMod = new RelayCommand(_ => StartCreateMod(), _ => CanCreateMod());

            RefreshMods = new RelayCommand(_ => RefreshExecute(), _ => IsNotBusy());

            AddFolder = new RelayCommand(_ => AddFolderExecute(), _ => IsNotBusy());

            RemoveFolder = new RelayCommand(_ => RemoveFolderExecute(), _ => CanRemoveFolder());

            SelectAll = new RelayCommand(_ => SelectAllExecute(), _ => IsNotBusy());
            SelectNone = new RelayCommand(_ => SelectNoneExecute(), _ => IsNotBusy());

            SaveLoadOrder = new RelayCommand(_ => SaveLoadOrderExecute(), _ => IsNotBusy());

            NewMod = "Compatibility SCAR's pathfinding fix";

            Folders.Add(OcsHelper.LocalFolders.Data);
            Folders.Add(OcsHelper.LocalFolders.Mod);

            Folders.CollectionChanged += (_, __) => RefreshExecute();

            RefreshExecute();
        }

        private void SelectNoneExecute()
        {
            Mods.ForEach(m => m.Selected = false);
        }

        private void SelectAllExecute()
        {
            Mods.ForEach(m => m.Selected = true);
        }

        private bool CanRemoveFolder() => CurrentFolder != null && IsNotBusy();

        private void RemoveFolderExecute()
        {
            Folders.Remove(CurrentFolder);
        }

        private bool CanCreateMod() => !Busy && !string.IsNullOrEmpty(NewMod) && Mods.Any(m => m.Selected) && FileHelper.IsValidPath(NewMod);

        private bool IsNotBusy() => !Busy;

        private void AddFolderExecute()
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select a new mod folder to use",
                ShowNewFolderButton = true,
            };

            var owner = Application.Current.MainWindow.AsWin32();

            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Folders.Add(new GameFolder(folder.SelectedPath));
            }
        }

        private void StartCreateMod()
        {
            Busy = true;

            Task.Run(CreateModExecute);
        }

        private void CreateModExecute()
        {
            try
            {
                var mods = Mods.Where(m => m.Selected).ToList();

                Folders.ForEach(f => f.Populate());

                var modPath = CreateNewMod();

                var data = OcsHelper.Load(mods.Select(m => m.Path), NewMod, Folders, resolveDependencies: false);

                var context = new PatchContext(data)
                {
                    Folders = Folders.ToArray(),
                    Mods = mods.ToDictionary(m => m.Name, m => m.Path),
                };

                new ScarPathfindingFixPatcher().Patch(context);

                data.save(modPath);

                if (MessageBox.Show($"Mod created successfully at {modPath}\nWould you like to add it to the game's load order?", "Mod created!", MessageBoxButton.YesNo)
                     == MessageBoxResult.Yes)
                {
                    var loadOrder = LoadOrder.Read().ToList();

                    if (string.IsNullOrWhiteSpace(Path.GetExtension(NewMod)))
                        loadOrder.Add(NewMod + ".mod");
                    else
                        loadOrder.Add(NewMod);
                }

                string CreateNewMod()
                {
                    var modNames = mods.Select(m => m.Name).ToList();

                    var header = new GameData.Header
                    {
                        Dependencies = modNames,
                        Referenced = new List<string>(),
                    };

                    return OcsHelper.NewMod(header, NewMod);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create mod with error message:{ex.Message}");
            }
            finally
            {
                Busy = false;
            }
        }

        private void RefreshExecute()
        {
            Folders.ForEach(f => f.Populate());

            var mods = Folders.SelectMany(f => f.Mods)
                             // Not the reference mod, the new mod or a base file
                             .Where(p => !OcsHelper.BaseMods.Contains(p.Key))
                             // Group by mod name
                             .GroupBy(p => p.Key, p => p.Value)
                             // Convert to dictionary by taking first item from group
                             .ToDictionary(g => g.Key, g => g.First());


            //.Select(p => new ModViewModel { Name = p.Key, Path = p.Value });

            Mods.Clear();

            foreach (var loadOrderItem in LoadOrder.Read().Where(i => mods.ContainsKey(i)))
            {
                Mods.Add(new ModViewModel { Name = loadOrderItem, Path = mods[loadOrderItem], Selected = true });

                mods.Remove(loadOrderItem);
            }

            mods.ForEach(p => Mods.Add(new ModViewModel { Name = p.Key, Path = p.Value }));
        }

        private void SaveLoadOrderExecute()
        {
            var mods = Mods.Where(m => m.Selected).Select(m => m.Name).ToArray();

            LoadOrder.Save(mods);
        }
    }
}
