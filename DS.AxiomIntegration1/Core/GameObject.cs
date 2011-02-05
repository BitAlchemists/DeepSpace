using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Math;
using DS.AxiomIntegration1.Core;

namespace DeepSpace.Core
{
    public class GameObject
    {
        public String Name { get; set; }
        List<GameObject> objects;
        List<IGameObjectComponent> components;
        //Dictionary<Type, List<IGameObjectComponent>> ComponentsByType;
        public Vector3 Position { get; set; }

        public GameObject()
        {
            Name = "";
            objects = new List<GameObject>();
            components = new List<IGameObjectComponent>();
            //ComponentsByType = new Dictionary<Type, List<GameObjectComponent>>();
            Position = new Vector3();
        }

        //Will add a component to the game objects components
        public void AddComponent(IGameObjectComponent component)
        {
            components.Add(component);
            //AddComponentToComponentsByTypeList(component);
        }

        //Will return a list of components by the given type
        //public List<GameObjectComponent> GetComponents(Type type)
        //{
        //    List<GameObjectComponent> components = null;
        //    ComponentsByType.TryGetValue(type, out components);
        //    return components;
        //}

        //private void AddComponentToComponentsByTypeList(GameObjectComponent component)
        //{
        //    List<GameObjectComponent> components = null;
        //    Type type = component.GetType();

        //    //Fetch the components list
        //    ComponentsByType.TryGetValue(type, out components);

        //    //if it does not exist, create it
        //    if (components == null)
        //    {
        //        components = new List<GameObjectComponent>();
        //        ComponentsByType.Add(type, components);
        //    }

        //    components.Add(component);
        //}

    }
}
