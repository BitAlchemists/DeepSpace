using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core;

namespace DeepSpace
{
    public abstract class Scene : IDisposable
    {
        protected List<GameObject> gameObjects;

        public Scene()
        {
            gameObjects = new List<GameObject>();
        }

        public abstract void LoadScene();
        public virtual void Update(float dT)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(dT);
            }
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
