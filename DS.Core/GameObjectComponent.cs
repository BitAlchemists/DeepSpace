using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.Core
{
    public class GameObjectComponent
    {
        public GameObject GameObject { get; protected set; }

        public GameObjectComponent(GameObject gameObject)
        {
            this.GameObject = gameObject;
        }

        public GameObjectComponent()
        {
            throw new Exception("This constructor is not supported");
        }
    }
}
