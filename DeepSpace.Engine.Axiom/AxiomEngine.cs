using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core.Interfaces;
using Axiom.Math;
using Axiom.Input;
using Axiom.Core;
using Axiom.Graphics;
using System.IO;
using Axiom.Overlays;
using System.Diagnostics;
using DeepSpace.Engine.Axiom.Configuration;
using Axiom.Utilities;
using Axiom.ParticleSystems;
using DeepSpace.Core;

namespace DeepSpace.Engine.Axiom
{
    public class AxiomEngine : IEngine, IDisposable
    {
        #region IEngine Members

        public FrameStartedHandler FrameStarted { get; set; }
        public FrameEndedHandler FrameEnded { get; set; }

        AxiomEngineComponentFactory componentFactory;
        public AxiomEngineComponentFactory ComponentFactory
        {
            get
            {
                return componentFactory;
            }
        }

        List<IEngineSubsystem> subsystems;
        public List<IEngineSubsystem> Subsystems 
        {
            get
            {
                return subsystems;
            }
        }

        public AxiomEngine()
        {
            SetupInput = new ConfigureInput(_setupInput);
            componentFactory = new AxiomEngineComponentFactory(this);
            subsystems = new List<IEngineSubsystem>();
        }

        public bool Setup()
        {
            // instantiate the Root singleton
            //engine = new Root( "AxiomEngine.log" );
            engine = new Root("EULog.txt");
            List<RenderSystem> renderSystems = new List<RenderSystem>(engine.RenderSystems.Values);
            //renderSystems[0]
            engine.RenderSystem = renderSystems[0];
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

            // add event handlers for frame events
            engine.FrameStarted += OnFrameStarted;
            engine.FrameRenderingQueued += OnFrameRenderingQueued;
            engine.FrameEnded += OnFrameEnded;

            window = Root.Instance.Initialize(true, "Axiom Engine Demo Window");

            TechDemoListener rwl = new TechDemoListener(window);
            WindowEventMonitor.Instance.RegisterListener(window, rwl);

            ChooseSceneManager();
            CreateCamera();
            CreateViewports();

            // set default mipmap level
            TextureManager.Instance.DefaultMipmapCount = 5;

            // Create any resource listeners (for loading screens)
            this.CreateResourceListener();
            // Load resources
            this.LoadResources();

            scene.AmbientLight = ColorEx.Gray;
            scene.SetSkyBox(true, "AR/SpaceSkyBox", 500);
            //scene.SetSkyDome(true, "Examples/CloudySky", 5, 8);

            ParticleSystem smokeSystem =
                ParticleSystemManager.Instance.CreateSystem("SmokeSystem", "Examples/Smoke");

            // create an entity to have follow the path
            // create a scene node for the entity and attach the entity
            /*
             *             Entity ogreHead = scene.CreateEntity("OgreHead", "razor.mesh");
            SceneNode headNode = scene.RootSceneNode.CreateChildSceneNode(Vector3.Zero, Quaternion.Identity);
            headNode.AttachObject(ogreHead);
*/
            Entity razorEntity = scene.CreateEntity("Razor", "razor.mesh");
            SceneNode razorNode = scene.RootSceneNode.CreateChildSceneNode(Vector3.Zero, Quaternion.Identity);
            razorNode.AttachObject(razorEntity);
            razorNode.Pitch(90);

            Entity ogreHead = scene.CreateEntity("OgreHead", "spacevessel.mesh");
            SceneNode headNode = scene.RootSceneNode.CreateChildSceneNode(new Vector3(300,0,0), Quaternion.Identity);
            headNode.AttachObject(ogreHead);


            scene.RootSceneNode.CreateChildSceneNode(new Vector3(0,50,0)).AttachObject(smokeSystem);



            input = SetupInput();

            return true;
        }

        public void StartRendering()
        {
            engine.StartRendering();
        }

        public void StopRendering()
        {
            engine.QueueEndRendering();
        }

        public void AddSubsystem(IEngineSubsystem subsystem)
        {
            subsystems.Add(subsystem);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (engine != null)
            {
                // remove event handlers
                engine.FrameStarted -= OnFrameStarted;
                engine.FrameEnded -= OnFrameEnded;

                //engine.Dispose();
            }
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

        #endregion

        public delegate InputReader ConfigureInput();
        public ConfigureInput SetupInput;

        #region Protected Fields

        protected Root engine;
        public Root Engine
        {
            get
            {
                return engine;
            }
            set
            {
                engine = value;
            }
        }
        protected Camera camera;
        internal Camera Camera
        {
            get
            {
                return camera;
            }
        }
        protected Viewport viewport;
        protected SceneManager scene;
        internal SceneManager Scene
        {
            get
            {
                return scene;
            }
        }
        protected RenderWindow window;
        internal RenderWindow Window
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
        protected InputReader input;
        internal InputReader Input
        {
            get
            {
                return input;
            }
        }
        #endregion Protected Fields

        #region Protected Methods

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

        protected void TakeScreenshot()
        {
            string[] temp = Directory.GetFiles(Environment.CurrentDirectory, "screenshot*.jpg");
            string fileName = string.Format("screenshot{0}.jpg", temp.Length + 1);
            window.WriteContentsToFile(fileName);
        }

        #endregion Protected Methods

        #region Protected Virtual Methods

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



        /// <summary>
        /// Optional override method where you can create resource listeners (e.g. for loading screens)
        /// </summary>
        protected virtual void CreateResourceListener()
        {
        }

        /// <summary>
        /// Optional override method where you can perform resource group loading
        /// </summary>
        /// <remarks>Must at least do ResourceGroupManager.Instance.InitializeAllResourceGroups();</remarks>
        protected virtual void LoadResources()
        {
            ResourceGroupManager.Instance.InitializeResourceGroup("bootstrap");
            ResourceGroupManager.Instance.InitializeResourceGroup("General");
            ResourceGroupManager.Instance.InitializeResourceGroup("AR");
        }

        protected InputReader _setupInput()
        {
            InputReader ir = null;
#if  !( XBOX || XBOX360 ) && !( SIS )
            // retrieve and initialize the input system
            ir = PlatformManager.Instance.CreateInputReader();
            ir.Initialize(window, true, true, false, false);
#endif

#if ( SIS )
            SharpInputSystem.ParameterList pl = new SharpInputSystem.ParameterList();
            pl.Add( new SharpInputSystem.Parameter( "WINDOW", this.window.Handle ) );

            //Default mode is foreground exclusive..but, we want to show mouse - so nonexclusive
            pl.Add( new SharpInputSystem.Parameter( "w32_mouse", "CLF_BACKGROUND" ) );
            pl.Add( new SharpInputSystem.Parameter( "w32_mouse", "CLF_NONEXCLUSIVE" ) );

            //This never returns null.. it will raise an exception on errors
            ir = SharpInputSystem.InputManager.CreateInputSystem( pl );
            mouse = ir.CreateInputObject<SharpInputSystem.Mouse>( true, "" );
            keyboard = ir.CreateInputObject<SharpInputSystem.Keyboard>( true, "" );
#endif
            return ir;
        }

#if USE_CEGUI
        protected virtual void CreateGUI()
        {
            // Next, we need a renderer. CeGui is not bound to any graphics API and
            // the renderers are the glue that let CeGui interface with a graphics API
            // like OpenGL (in this case) or a 3D engine like Axiom.
            guiRenderer = new CeGui.Renderers.Axiom.Renderer( window, RenderQueueGroupID.Overlay, false, scene );

            // Initialize the CeGui system. This should be the first method called before
            // using any of the CeGui routines.
            CeGui.GuiSystem.Initialize( guiRenderer );

            // All graphics used by any CeGui themes are stored in image files that are mapped
            // by a special XML description which tells CeGui what can be found where on the
            // images. Obviously, we need to load these resources
            //
            // Note that it is possible, and even usual, for these steps to
            // be done automatically via a "scheme" definition, or even from the
            // cegui.conf configuration file, however for completeness, and as an
            // example, virtually everything is being done manually in this example
            // code.

            // Widget sets are collections of widgets that provide the widget classes defined
            // in CeGui (like PushButton, CheckBox and so on) with their own distinctive look
            // (like a theme) and possibly even custom behavior.
            //
            // Here we load all compiled widget sets we can find in the current directory. This
            // is done to demonstrate how you could add widget set dynamically to your
            // application. Other possibilities would be to hardcode the widget set an
            // application uses or determining the assemblies to load from a configuration file.
            string[] assemblyFiles = System.IO.Directory.GetFiles(
              System.IO.Directory.GetCurrentDirectory(), "CeGui.WidgetSets.*.dll"
            );
            foreach ( string assemblyFile in assemblyFiles )
            {
                CeGui.WindowManager.Instance.AttachAssembly(
                  System.Reflection.Assembly.LoadFile( assemblyFile )
                );
            }

            // Imagesets area a collection of named areas within a texture or image file. Each
            // area becomes an Image, and has a unique name by which it can be referenced. Note
            // that an Imageset would normally be specified as part of a scheme file, although
            // as this example is demonstrating, it is not a requirement.
            //
            // Again, we load all image sets we can find, this time searching the resources folder.
            string[] imageSetFiles = System.IO.Directory.GetFiles(
              System.IO.Directory.GetCurrentDirectory() + "\\Resources", "*.imageset"
            );

            foreach ( string imageSetFile in imageSetFiles )
                CeGui.ImagesetManager.Instance.CreateImageset( imageSetFile );

            // When the gui imagery side of thigs is set up, we should load in a font.
            // You should always load in at least one font, this is to ensure that there
            // is a default available for any gui element which needs to draw text.
            // The first font you load is automatically set as the initial default font,
            // although you can change the default later on if so desired.  Again, it is
            // possible to list fonts to be automatically loaded as part of a scheme, so
            // this step may not usually be performed explicitly.
            //
            // Fonts are loaded via the FontManager singleton.
            CeGui.FontManager.Instance.CreateFont( "Default", "Arial", 9, CeGui.FontFlags.None );
            CeGui.FontManager.Instance.CreateFont( "WindowTitle", "Arial", 12, CeGui.FontFlags.Bold );
            CeGui.GuiSystem.Instance.SetDefaultFont( "Default" );

            // The next thing we do is to set a default mouse cursor image.  This is
            // not strictly essential, although it is nice to always have a visible
            // cursor if a window or widget does not explicitly set one of its own.
            //
            // This is a bit hacky since we're assuming the SuaveLook image set, referenced
            // below, will always be available.
            CeGui.GuiSystem.Instance.SetDefaultMouseCursor(
              CeGui.ImagesetManager.Instance.GetImageset( "SuaveLook" ).GetImage( "Mouse-Arrow" )
            );

            // Now that the system is initialised, we can actually create some UI elements,
            // for this first example, a full-screen 'root' window is set as the active GUI
            // sheet, and then a simple frame window will be created and attached to it.
            //
            // All windows and widgets are created via the WindowManager singleton.
            CeGui.WindowManager winMgr = CeGui.WindowManager.Instance;

            // Here we create a "DefaultWindow". This is a native type, that is, it does not
            // have to be loaded via a scheme, it is always available. One common use for the
            // DefaultWindow is as a generic container for other windows. Its size defaults
            // to 1.0f x 1.0f using the relative metrics mode, which means when it is set as
            // the root GUI sheet window, it will cover the entire display. The DefaultWindow
            // does not perform any rendering of its own, so is invisible.
            //
            // Create a DefaultWindow called 'Root'.
            rootGuiSheet = winMgr.CreateWindow( "DefaultWindow", "Root" ) as CeGui.GuiSheet;

            // Set the GUI root window (also known as the GUI "sheet"), so the gui we set up
            // will be visible.
            CeGui.GuiSystem.Instance.GuiSheet = rootGuiSheet;


            // Add the dialog as child to the root gui sheet. The root gui sheet is the desktop
            // and we've just added a window to it, so the window will appear on the desktop.
            // Logical, right?
            rootGuiSheet.AddChild( new Gui.DebugRTTWindow(
                                    //new CeGui.WidgetSets.Suave.SuaveGuiBuilder()
                                    //new CeGui.WidgetSets.Taharez.TLGuiBuilder()
                                    new CeGui.WidgetSets.Windows.WLGuiBuilder()
                                ));
        }
#endif
        #endregion Protected Virtual Methods

        #region Public Methods




        #endregion Public Methods

        protected virtual void OnFrameStarted(object source, FrameEventArgs evt)
        {
            // TODO: Move this into an event queueing mechanism that is processed every frame
            input.Capture();

            foreach (IEngineSubsystem subsystem in subsystems)
            {
                subsystem.Update(evt.TimeSinceLastFrame);
            }

            if (FrameStarted != null)
            {
                FrameStarted(evt.TimeSinceLastFrame);
            }
        }

        protected virtual void OnFrameRenderingQueued(object source, FrameEventArgs evt)
        {
        }

        protected virtual void OnFrameEnded(object source, FrameEventArgs evt)
        {
            if (FrameEnded != null)
            {
                FrameEnded();
            }
        }
    }

    public class TechDemoListener : IWindowEventListener
    {
        private RenderWindow _mw;
        public TechDemoListener(RenderWindow mainWindow)
        {
            Contract.RequiresNotNull(mainWindow, "mainWindow");

            _mw = mainWindow;
        }

        /// <summary>
        /// Window has moved position
        /// </summary>
        /// <param name="rw">The RenderWindow which created this event</param>
        public void WindowMoved(RenderWindow rw)
        {
        }

        /// <summary>
        /// Window has resized
        /// </summary>
        /// <param name="rw">The RenderWindow which created this event</param>
        public void WindowResized(RenderWindow rw)
        {
        }

        /// <summary>
        /// Window has closed
        /// </summary>
        /// <param name="rw">The RenderWindow which created this event</param>
        public void WindowClosed(RenderWindow rw)
        {
            Contract.RequiresNotNull(rw, "RenderWindow");

            // Only do this for the Main Window
            if (rw == _mw)
            {
                Root.Instance.QueueEndRendering();
            }
        }

        /// <summary>
        /// Window lost/regained the focus
        /// </summary>
        /// <param name="rw">The RenderWindow which created this event</param>
        public void WindowFocusChange(RenderWindow rw)
        {
        }

    }
}
