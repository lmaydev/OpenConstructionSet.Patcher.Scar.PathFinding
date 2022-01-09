using AutoFixture;
using AutoFixture.AutoNSubstitute;
using System.Windows;

namespace OpenConstructionSet.Patcher.Scar.PathFinding
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static readonly IFixture Fixture;

        static App()
        {
            Fixture = new Fixture().Customize(new AutoNSubstituteCustomization {  ConfigureMembers=true});
        }
    }
}
