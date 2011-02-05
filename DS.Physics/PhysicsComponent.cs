using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.Core;
using DS.Math;

namespace DS.Physics
{
    class PhysicsComponent : GameObjectComponent
    {
        public double Mass { get; set; }
        public Vector3 Force { get; set; }
        public Vector3 Velocity { get; set; }
    }
}
