using AutoFixture;
using OpenConstructionSet.Patcher.Scar.PathFinding.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point start;

        public MainWindow()
        {
            var fixture = new Fixture();

            var vm = fixture.Create<InstallationViewModel>();

            InitializeComponent();
        }
    }
}