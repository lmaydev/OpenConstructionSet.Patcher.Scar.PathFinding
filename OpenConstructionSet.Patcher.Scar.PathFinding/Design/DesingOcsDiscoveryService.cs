using AutoFixture;
using OpenConstructionSet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenConstructionSet.Patcher.Scar.PathFinding.Design
{
    internal class DesingOcsDiscoveryService : IOcsDiscoveryService
    {
        public string[] SupportedFolderLocators => throw new NotImplementedException();

        public Dictionary<string, Installation> DiscoverAllInstallations() => App.Fixture.Create<Dictionary<string, Installation>>();

        public Installation? DiscoverInstallation()
        {
            throw new NotImplementedException();
        }

        public Installation? DiscoverInstallation(string locatorId)
        {
            throw new NotImplementedException();
        }

        public ModFile? DiscoverMod(string file)
        {
            throw new NotImplementedException();
        }

        public ModFolder? DiscoverModFolder(string folder)
        {
            throw new NotImplementedException();
        }

        public Save? DiscoverSave(string folder)
        {
            throw new NotImplementedException();
        }

        public SaveFolder? DiscoverSaveFolder(string folder)
        {
            throw new NotImplementedException();
        }

        public Header? ReadHeader(string path)
        {
            throw new NotImplementedException();
        }
    }
}

