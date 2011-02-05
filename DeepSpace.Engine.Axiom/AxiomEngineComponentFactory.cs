using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpace.Engine.Axiom
{
    public class AxiomEngineComponentFactory
    {
        AxiomEngine engine;

        private AxiomEngineComponentFactory()
        {
            throw new Exception("Cannot ctor AxiomEngineComponentFactory without params");
        }

        internal AxiomEngineComponentFactory(AxiomEngine engine)
        {
            this.engine = engine;
        }

        public CameraComponent CreateCameraComponent()
        {
            CameraComponent component = new CameraComponent();
            component.Camera = engine.Camera;
            return component;
        }

        public GraphicalComponent CreateGraphicalComponent()
        {
            GraphicalComponent component = new GraphicalComponent();
            return component;
        }

        public OverlayComponent CreateOverlayComponent()
        {
            OverlayComponent component = new OverlayComponent();
            return component;
        }

        public DebugOverlayComponent CreateDebugOverlayComponent()
        {
            DebugOverlayComponent component = new DebugOverlayComponent(engine);
            return component;
        }

        public LightComponent CreateLightComponent()
        {
            LightComponent component = new LightComponent();
            return component;
        }

        public InputComponent CreateInputComponent()
        {
            InputComponent component = new InputComponent();
            component.Input = engine.Input;
            return component;
        }
    }
}
