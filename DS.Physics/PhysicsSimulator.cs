using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.Core;
using DS.Math;

namespace DS.Physics
{
    class PhysicsSimulator
    {
        private List<PhysicsComponent> Components;
        private const double GravityConstant = 6.7 * 10e-12;
        private const double AstronomicalUnit = 149597870691.0;
        public double TimeFactor { get; set; }
        public int SimulationsPerFrame { get; set; }
        public bool Pause { get; set; }

        public PhysicsSimulator()
        {
            TimeFactor = 60 * 60 * 24 * 5;
            SimulationsPerFrame = 33;
            Pause = false;
        }

        bool Start()
        {
            Init();
            return true;
        }

        void Update(double dT)
        {
            if (Pause)
                return;

            dT = dT * TimeFactor / SimulationsPerFrame;
            for (int i = 0; i < SimulationsPerFrame; ++i)
            {
                if (Components.Count > 1)
                    CalcGravity();
                Simulate(dT);
                Init();
            }
        }

        void Stop()
        {

        }

        void Init()
        {
            foreach(PhysicsComponent component in Components)
                component.Force.Reset();
        }

        void CalcGravity()
        {
            Vector3 vcDistance;  // abstand der 2 objekte
            Vector3 vcForce;
            double dDistance;          // abstand in astronomischen einheiten
            double dForce;            // kraft in newton
            for (int i = 0; i < Components.Count; ++i) // für alle objekte
            {
                PhysicsComponent componentI = Components[i];
                for (int j = i + 1; j < Components.Count; ++j) // berechnet mit allen anderen objekten
                {
                    PhysicsComponent componentJ = Components[j];

                    vcDistance = componentJ.GameObject.Position - componentI.GameObject.Position; // abstandsvektor ermitteln
                    dDistance = vcDistance.Length; // und die absolute länge
                    if (dDistance == 0) dDistance = 0.1;
                    // F = G * m1 * m2 / r²
                    dForce = (GravityConstant * componentI.Mass * componentJ.Mass) / (dDistance * dDistance);

                    //now we want to transform this into a force vector
                    vcDistance.Normalize();
                    vcForce = vcDistance * dForce;

                    componentI.Force = componentI.Force + vcForce; // kraft auf objekt anwenden
                    componentJ.Force = componentJ.Force + vcForce; // und umgekehrt auf das andere
                }
            }
        }

        void Simulate(double dT)
        {
            foreach(PhysicsComponent component in Components)
            {
                component.Velocity = component.Velocity + component.Force / (component.Mass * dT);
                component.GameObject.Position = component.GameObject.Position + component.Velocity * dT;
            }
        }
    }
}
    