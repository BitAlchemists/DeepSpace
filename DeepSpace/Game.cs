using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core.Logging;
using DeepSpace.Core.Interfaces;
using DeepSpace.Engine.Axiom;
using DeepSpace.Core;

namespace DeepSpace
{
    public abstract class Game
    {
        protected IEngine engine;
        public IEngine Engine
        {
            get
            {
                return engine;
            }
        }

        public Game()
        {
            engine = new AxiomEngine();
        }

        public virtual void Start()
        {
            //LoadingScreen anzeigen
            //Setup der Engine
            //Registrieren für Events
            //In den Start-State eintreten

            //try
            {
                if (!engine.Setup())
                {
                    LogManager.Instance.Write("Engine failed to setup");
                    return;
                }
                if (!this.Setup())
                {
                    LogManager.Instance.Write("Game failed to setup");
                    return;
                }

                // start the engines rendering loop
                this.BeginGame();
                engine.StartRendering();
            }
            //catch (Exception ex)
            //{
            //    // try logging the error here first, before Root is disposed of
            //    if (LogManager.Instance != null)
            //    {
            //        LogManager.Instance.Write(LogManager.BuildExceptionString(ex));
            //    }
            //}
        }

        protected bool Setup()
        {
            engine.FrameStarted = OnFrameStarted;
            engine.FrameEnded = OnFrameEnded;
            return true;
        }

        protected abstract void BeginGame();
        protected abstract void OnFrameStarted(float dT);
        protected abstract void OnFrameEnded();
    }
}
