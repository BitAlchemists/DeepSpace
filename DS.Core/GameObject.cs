using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.Math;

namespace DS.Core
{
    public class GameObject
    {
        public String Name { get; set; }
        List<GameObjectComponent> Components;
        Dictionary<Type, List<GameObjectComponent>> ComponentsByType;
        public Vector3 Position { get; set; }

        public GameObject()
        {
            Name = "";
            Components = new List<GameObjectComponent>();
            ComponentsByType = new Dictionary<Type, List<GameObjectComponent>>();
            Position = new Vector3();
        }

        //Will add a component to the game objects components
        public void AddComponent(GameObjectComponent component)
        {
            Components.Add(component);
            AddComponentToComponentsByTypeList(component);
        }

        //Will return a list of components by the given type
        public List<GameObjectComponent> GetComponents(Type type)
        {
            List<GameObjectComponent> components = null;
            ComponentsByType.TryGetValue(type, out components);
            return components;
        }

        private void AddComponentToComponentsByTypeList(GameObjectComponent component)
        {
            List<GameObjectComponent> components = null;
            Type type = component.GetType();

            //Fetch the components list
            ComponentsByType.TryGetValue(type, out components);

            //if it does not exist, create it
            if (components == null)
            {
                components = new List<GameObjectComponent>();
                ComponentsByType.Add(type, components);
            }

            components.Add(component);
        }

    }
}

/*
    IObject();
    virtual ~IObject();

    void Update(){}

    // accessor methods
    Vector4D GetPos(void)       { return vcPos;   }
    Vector4D GetRight(void)     { return vcRight; }
    Vector4D GetUp(void)        { return vcUp;    }
    Vector4D GetDir(void)       { return vcDir;   }
    Vector4D GetVelocity(void)  { return vcVelocity;     }
    Matrix* GetpMatrix()        { return &mat;           }


    //AUTO_SIZE;
    
public:
    int ID;
    static int IDcount;
    static int FetchID();
    
    double mass;
    char* name;
    
    Vector4D vcPos;      // position
    Vector4D vcRight;    // right vector
    Vector4D vcUp;       // up vector
    Vector4D vcDir;      // direction vector (look direction)
    Vector4D vcForce;
    Vector4D vcVelocity;        // velocity vector (movement direction)
    Matrix      mat;
    Quaternion   Quat;       // quaternion for rotation

    // rotation speed around local vectors
    float     fRollSpd;
    float     fPitchSpd;
    float     fYawSpd;

    float     fRollSpdMax;
    float     fPitchSpdMax;
    float     fYawSpdMax;

    // rotation value around local vectors
    float     fRotX;
    float     fRotY;
    float     fRotZ;
    
    // other stuff
    float fThrust;

    // protected methods
    virtual void RecalcAxes(void);
    virtual void Init(void);
    virtual void Render();
*/