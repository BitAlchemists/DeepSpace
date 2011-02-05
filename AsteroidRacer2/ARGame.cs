using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace;
using DeepSpace.Core;
using DeepSpace.Engine.Axiom;
using AsteroidRacer2.Physics;
using Axiom.Input;
using DeepSpace.Core.Logging;

namespace AsteroidRacer2
{
    public class ARGame : Game
    {
        ARScene scene;
        InputComponent quitter;
        protected ARComponentFactory componentFactory;
        public ARComponentFactory ComponentFactory
        {
            get
            {
                return componentFactory;
            }
        }

        public ARGame() : base()
        {
            componentFactory = new ARComponentFactory();
            componentFactory.GraphicsInputFactory = ((AxiomEngine)engine).ComponentFactory;
            PhysicsEngine physicsEngine = new PhysicsEngine();
            engine.AddSubsystem(physicsEngine);
            componentFactory.PhysicalFactory = physicsEngine.ComponentFactory;
            new LogManager();
            LogManager.Instance.CreateLog("AR.log", true);
        }

        protected override void BeginGame()
        {
            scene = new ARScene(this);
            scene.LoadScene();
            quitter = componentFactory.GraphicsInputFactory.CreateInputComponent();
            quitter.AddKeyDelegate(KeyCodes.Escape, new InputComponent.InputHandler(RequestEndGame));
        }

        private void RequestEndGame()
        {
            engine.StopRendering();
        }

        protected override void OnFrameStarted(float dT)
        {
            quitter.Update(dT);
            scene.Update(dT);
        }

        protected override void OnFrameEnded()
        {
            
        }
    }
}
