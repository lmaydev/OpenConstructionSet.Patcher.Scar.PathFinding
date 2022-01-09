namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ModViewModel : BaseViewModel
    {
        private bool selected;

        public string Name { get; }

        public string Path { get; }

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

        public ModViewModel(string name, string path, bool selected)
        {
            Name = name;
            Path = path;
            Selected = selected;
        }
    }
}