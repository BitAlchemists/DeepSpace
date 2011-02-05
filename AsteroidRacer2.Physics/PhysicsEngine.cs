using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core.Interfaces;

namespace AsteroidRacer2.Physics
{
    public class PhysicsEngine : IEngineSubsystem
    {
        PhysicalComponentFactory componentFactory;
        public PhysicalComponentFactory ComponentFactory
        {
            get
            {
                return componentFactory;
            }
        }

        public PhysicsEngine()
        {
            componentFactory = new PhysicalComponentFactory();
        }

        public void Update(float dT)
        {

        }
    }
}
