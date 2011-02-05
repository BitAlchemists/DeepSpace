using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.AxiomIntegration1.Core;

namespace DeepSpace.Core
{
    public interface IEngineSubsystem : IDisposable
    {
        string Name { get; }
        int Rank { get; set; }
        IGameObjectComponentFactory GameObjectComponentFactory { get; }

        bool Initialize();
        bool Start();
        bool Update();
        bool Stop();
        bool DeInitialize();
    }
}
