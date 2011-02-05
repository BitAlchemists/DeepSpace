using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Engine.Axiom;
using AsteroidRacer2.Physics;

namespace AsteroidRacer2
{
    public class ARComponentFactory
    {
        public AxiomEngineComponentFactory GraphicsInputFactory { get; set; }
        public PhysicalComponentFactory PhysicalFactory { get; set; }

        public GraphicalComponent CreateGraphicalComponent()
        {
            return GraphicsInputFactory.CreateGraphicalComponent();
        }

        public OverlayComponent CreateOverlayComponent()
        {
            return GraphicsInputFactory.CreateOverlayComponent();
        }

        public LightComponent CreateLightComponent()
        {
            return GraphicsInputFactory.CreateLightComponent();
        }

        public InputComponent CreateInputComponent()
        {
            return GraphicsInputFactory.CreateInputComponent();
        }

        public PhysicalComponent CreatePhysicalComponent()
        {
            return PhysicalFactory.CreatePhysicalComponent();
        }

        public CameraComponent CreateCameraComponent()
        {
            return GraphicsInputFactory.CreateCameraComponent();
        }

        public DebugOverlayComponent CreateDebugOverlayComponent()
        {
            return GraphicsInputFactory.CreateDebugOverlayComponent();
        }
    }
}
