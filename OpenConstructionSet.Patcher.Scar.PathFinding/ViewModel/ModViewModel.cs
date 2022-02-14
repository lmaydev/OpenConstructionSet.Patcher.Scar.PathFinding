using OpenConstructionSet.Data;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ModViewModel : BaseViewModel
    {
        private bool selected;

        public ModViewModel(string name, string path, Header header, bool selected)
        {
            Name = name;
            Path = path;
            Header = header;
            Selected = selected;
        }

        public Header Header { get; }

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
    }
}