using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core;

namespace DS.AxiomIntegration1.Core
{
    public interface IGameObjectComponent
    {
        GameObject GameObject { get; set; }
    }
}
