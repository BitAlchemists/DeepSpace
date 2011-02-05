using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.AxiomIntegration1.Core
{
    public interface IGameObjectComponentFactory
    {
        IGameObjectComponent CreateComponent();
    }
}
