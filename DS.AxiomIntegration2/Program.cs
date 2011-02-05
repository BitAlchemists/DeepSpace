using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Core;
using Axiom.Graphics;
using System.IO;
using Axiom.Demos.Configuration;

namespace DS.AxiomIntegration2
{
    class Program
    {
        static void Main(string[] args)
        {
            AxiomTutorial app = new AxiomTutorial();
            Root engine = new Root("EULog.txt");
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

            /*
            ConfigDialog dlg = new ConfigDialog();
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return;
            }
            */
             
            app.SetupResources();
            app.Start();
        }


    }

    class AxiomTutorial : TechDemo
    {
        override public void CreateScene()
        {

        }
    }
}
