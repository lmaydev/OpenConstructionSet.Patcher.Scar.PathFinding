using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel
{
    internal class ModViewModel : BaseViewModel
    {
        private bool selected;

        public string Name { get; set; }

        public string Path { get; set; }

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