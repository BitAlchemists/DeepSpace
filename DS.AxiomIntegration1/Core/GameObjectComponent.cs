using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpace.Core
{
    public abstract class GameObjectComponent
    {
        public GameObject GameObject { 
            get;
            protected set;
        }

        public GameObjectComponent(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }
    }
}
