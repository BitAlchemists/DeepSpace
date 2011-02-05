using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidRacer2.Physics
{
    public class PhysicalComponentFactory
    {
        public PhysicalComponent CreatePhysicalComponent()
        {
            PhysicalComponent component = new PhysicalComponent();
            return component;
        }
    }
}
