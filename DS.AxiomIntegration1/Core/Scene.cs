using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.AxiomIntegration1.Core;

namespace DeepSpace.Core
{
    public abstract class Scene
    {
        protected List<GameObject> gameObjects;
        protected GameBase Game { get; set; }

        public Scene(GameBase game)
        {
            gameObjects = new List<GameObject>();
            Game = game;
        }

        public abstract void LoadScene();
    }
}
