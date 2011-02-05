using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace;
using DeepSpace.Core;
using DeepSpace.Engine.Axiom;
using AsteroidRacer2.Physics;
using Axiom.Input;

namespace AsteroidRacer2
{
    class ARScene : Scene
    {
        ARGame game;

        public ARScene()
        {
            throw new Exception("Cannot ctor ARScene without parameters");
        }

        public ARScene(ARGame game) : base()
        {
            this.game = game;
        }

        public override void LoadScene()
        {
            //Create Root Component
            GameObject root = new GameObject("Root");
            gameObjects.Add(root);
            InputComponent rootInput = game.ComponentFactory.CreateInputComponent();
            root.AddComponent(rootInput);

            CameraComponent camera = game.ComponentFactory.CreateCameraComponent();
            root.AddComponent(camera);
            rootInput.AddKeyDelegate(KeyCodes.A, new InputComponent.InputHandler(camera.MoveLeft));
            rootInput.AddKeyDelegate(KeyCodes.D, new InputComponent.InputHandler(camera.MoveRight));
            rootInput.AddKeyDelegate(KeyCodes.W, new InputComponent.InputHandler(camera.MoveForward));
            rootInput.AddKeyDelegate(KeyCodes.S, new InputComponent.InputHandler(camera.MoveBackward));
            rootInput.AddKeyDelegate(KeyCodes.R, new InputComponent.InputHandler(camera.MoveUp));
            rootInput.AddKeyDelegate(KeyCodes.F, new InputComponent.InputHandler(camera.MoveDown));
            rootInput.AddKeyDelegate(KeyCodes.Left, new InputComponent.InputHandler(camera.YawLeft));
            rootInput.AddKeyDelegate(KeyCodes.Right, new InputComponent.InputHandler(camera.YawRight));
            rootInput.AddKeyDelegate(KeyCodes.Up, new InputComponent.InputHandler(camera.PitchUp));
            rootInput.AddKeyDelegate(KeyCodes.Down, new InputComponent.InputHandler(camera.PitchDown));
            rootInput.AddKeyDelegate(KeyCodes.Q, new InputComponent.InputHandler(camera.RollLeft));
            rootInput.AddKeyDelegate(KeyCodes.E, new InputComponent.InputHandler(camera.RollRight));

            //Add screen overlay
            DebugOverlayComponent overlay = game.ComponentFactory.CreateDebugOverlayComponent();
            root.AddComponent(overlay);

            overlay.ShowOverlay(true);
            rootInput.AddToggleableKeyDelegate(KeyCodes.D1, new InputComponent.InputHandler(overlay.ToggleOverlay));

            //Player vehicle
            GameObject vessel = new GameObject("Vessel");
            gameObjects.Add(vessel);

            PhysicalComponent physical = game.ComponentFactory.CreatePhysicalComponent();
            //physical.type = PhysicalType.Sphere;
            vessel.AddComponent(physical);

            GraphicalComponent graphical = game.ComponentFactory.CreateGraphicalComponent();
            //graphical.mesh = Mesh.Vessel;
            vessel.AddComponent(graphical);


            for (int i = 0; i < 10; i++)
            {
                GameObject asteroid = new GameObject("Asteroid" + i);
                gameObjects.Add(asteroid);

                physical = game.ComponentFactory.CreatePhysicalComponent();
                //physical.type = PhysicalType.Sphere;
                asteroid.AddComponent(physical);

                graphical = game.ComponentFactory.CreateGraphicalComponent();
                //graphical.mesh = Mesh.Asteroid;
                vessel.AddComponent(graphical);
            }
        }
    }
}
