using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Graphics;
using Axiom.Core;
using System.IO;
using Axiom.Math;
using System.Diagnostics;
using Axiom.Overlays;
using DeepSpace.Core;
using DeepSpace.Engine.Axiom.Configuration;

namespace DeepSpace.Engine.Axiom
{
    public class GraphicsManager : IDisposable
    {
        protected Root engine;
        public Root Engine
        {
            get
            {
                return engine;
            }
        }
        protected Camera camera;
        public Camera Camera
        {
            get
            {
                return camera;
            }
        }
        protected Viewport viewport;
        protected SceneManager scene;
        public SceneManager Scene
        {
            get
            {
                return scene;
            }
        }
        protected RenderWindow window;
        public RenderWindow Window
        {
            get
            {
                return window;
            }
            set
            {
                window = value;
            }
        }
        protected int aniso = 1;
        protected TextureFiltering filtering = TextureFiltering.Bilinear;

        public event EventHandler<FrameEventArgs> FrameStarted;
        public event EventHandler<FrameEventArgs> FrameEnded;


        public virtual bool Setup()
        {
            engine = new Root("EULog.txt");
            //Select any render system
            List<RenderSystem> renderSystems = new List<RenderSystem>(engine.RenderSystems.Values);
            engine.RenderSystem = renderSystems[0];

            LoadEngineConfig();

            window = Root.Instance.Initialize(true, "Axiom Engine Demo Window");

            WindowEventListener rwl = new WindowEventListener(window);
            WindowEventMonitor.Instance.RegisterListener(window, rwl);

            ChooseSceneManager();
            CreateCamera();
            CreateViewports();

            // set default mipmap level
            TextureManager.Instance.DefaultMipmapCount = 5;


            // add event handlers for frame events
            engine.FrameStarted += OnFrameStarted;
            engine.FrameRenderingQueued += OnFrameRenderingQueued;
            engine.FrameEnded += OnFrameEnded;

            ShowDebugOverlay(true);

            return true;
        }

        public void OnFrameStarted(object sender, FrameEventArgs evt)
        {
            if(this.FrameStarted != null)
                this.FrameStarted(sender, evt);
        }

        public void OnFrameRenderingQueued(object sender, FrameEventArgs evt)
        {

        }

        public void OnFrameEnded(object sender, FrameEventArgs evt)
        {
            if (this.FrameEnded != null)
                this.FrameEnded(sender, evt);
        }

        

        protected virtual void LoadEngineConfig()
        {
            const string CONFIG_FILE = @"EngineConfig.xml";
            string resourceConfigPath = Path.GetFullPath(CONFIG_FILE);

            if (File.Exists(resourceConfigPath))
            {
                EngineConfig config = new EngineConfig();

                // load the config file
                // relative from the location of debug and releases executables
                config.ReadXml(CONFIG_FILE);

                // interrogate the available resource paths
                foreach (EngineConfig.FilePathRow row in config.FilePath)
                {
                    ResourceGroupManager.Instance.AddResourceLocation(Path.GetFullPath(row.src), row.type, row.group);
                }
            }
        }

        public virtual void ChooseSceneManager()
        {
            // Get the SceneManager, a generic one by default
            scene = engine.CreateSceneManager("DefaultSceneManager", "TechDemoSMInstance");
            scene.ClearScene();
        }

        public virtual void CreateViewports()
        {
            Debug.Assert(window != null, "Attempting to use a null RenderWindow.");

            // create a new viewport and set it's background color
            viewport = window.AddViewport(camera, 0, 0, 1.0f, 1.0f, 100);
            viewport.BackgroundColor = ColorEx.Black;
        }

        public virtual void CreateCamera()
        {
            // create a camera and initialize its position
            camera = scene.CreateCamera("MainCamera");
            camera.Position = new Vector3(0, 0, 500);
            camera.LookAt(new Vector3(0, 0, -300));

            // set the near clipping plane to be very close
            camera.Near = 5;

            camera.AutoAspectRatio = true;
        }

        /// <summary>
        ///    Shows the debug overlay, which displays performance statistics.
        /// </summary>
        public void ShowDebugOverlay(bool show)
        {
            // gets a reference to the default overlay
            Overlay o = OverlayManager.Instance.GetByName("Core/DebugOverlay");

            if (o == null)
            {
                LogManager.Instance.Write(string.Format("Could not find overlay named '{0}'.", "Core/DebugOverlay"));
                return;
            }

            if (show)
            {
                o.Show();
            }
            else
            {
                o.Hide();
            }
        }

        public void TakeScreenshot(string fileName)
        {
            window.WriteContentsToFile(fileName);
        }

        public void StartRendering()
        {
            engine.StartRendering();
        }

        #region IDisposable Members

        public void Dispose()
        {
            engine.FrameEnded -= OnFrameEnded;
            engine.FrameRenderingQueued -= OnFrameRenderingQueued;
            engine.FrameStarted -= OnFrameStarted;

            if (scene != null)
                scene.RemoveAllCameras();
            camera = null;
            if (Root.Instance != null)
                Root.Instance.RenderSystem.DetachRenderTarget(window);
            if (window != null)
                window.Dispose();
            if (engine != null)
                engine.Dispose();
        }

        public PolygonMode TogglePolygonMode()
        {
            if (camera.PolygonMode == PolygonMode.Points)
            {
                camera.PolygonMode = PolygonMode.Solid;
            }
            else if (camera.PolygonMode == PolygonMode.Solid)
            {
                camera.PolygonMode = PolygonMode.Wireframe;
            }
            else
            {
                camera.PolygonMode = PolygonMode.Points;
            }

            return camera.PolygonMode;
        }

        public TextureFiltering ToggleTextureFiltering()
        {
            switch (filtering)
            {
                case TextureFiltering.Bilinear:
                    filtering = TextureFiltering.Trilinear;
                    aniso = 1;
                    break;
                case TextureFiltering.Trilinear:
                    filtering = TextureFiltering.Anisotropic;
                    aniso = 8;
                    break;
                case TextureFiltering.Anisotropic:
                    filtering = TextureFiltering.Bilinear;
                    aniso = 1;
                    break;
            }

            // set the new default
            MaterialManager.Instance.SetDefaultTextureFiltering(filtering);
            MaterialManager.Instance.DefaultAnisotropy = aniso;

            return filtering;
        }

        public bool ToggleShowBoundingBoxes()
        {
            scene.ShowBoundingBoxes = !scene.ShowBoundingBoxes;
            return scene.ShowBoundingBoxes;
        }

        public void ToggleShowOverlays()
        {
            viewport.ShowOverlays = !viewport.ShowOverlays;
        }

        #endregion
    }
}
