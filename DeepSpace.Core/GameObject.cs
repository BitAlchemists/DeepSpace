using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpace.Core
{
    public class GameObject
    {
        public string Name { get; set; }
        protected List<GameObjectComponent> components;

        public GameObject() : this("") { }

        public GameObject(string name)
        {
            Name = name;
            components = new List<GameObjectComponent>();
        }

        public void AddComponent(GameObjectComponent component)
        {
            components.Add(component);
        }

        public void Update(float dT)
        {
            foreach (GameObjectComponent component in components)
            {
                component.Update(dT);
            }
        }
    }
}
