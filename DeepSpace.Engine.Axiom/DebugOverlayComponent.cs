using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Overlays;
using Axiom.Core;
using DeepSpace.Core.Interfaces;

namespace DeepSpace.Engine.Axiom
{
    public class DebugOverlayComponent : OverlayComponent
    {
        DateTime averageStart = DateTime.Now;
        float sum = 0;
        float average = 0;
        int elapsedFrames = 1;
        AxiomEngine engine;
        string debugText = "";
        float debugTextFadeDelay;
        public string DebugText
        {
            get
            {
                return debugText;
            }
            set
            {
                debugTextFadeDelay = 5.0f;
                SetDebugText(value);
            }
        }

        void SetDebugText(string text)
        {
            debugText = text;

            OverlayElement element = OverlayManager.Instance.Elements.GetElement("Core/DebugText");
            if (element != null)
            {
                element.Text = debugText;
            }
        }

        public DebugOverlayComponent(AxiomEngine engine) : base()
        {
            Name = "Core/DebugOverlay";
            this.engine = engine;
        }

        public override void Update(float dT)
        {
            if (debugTextFadeDelay > 0)
            {
                debugTextFadeDelay -= dT;
                if (debugTextFadeDelay <= 0.0f)
                {
                    SetDebugText("");
                }
            }

            OverlayElement element = OverlayManager.Instance.Elements.GetElement("Core/CurrFps");
            if (element != null)
                element.Text = string.Format("Current FPS: {0:#.00}", Root.Instance.CurrentFPS);

            element = OverlayManager.Instance.Elements.GetElement("Core/BestFps");
            if (element != null)
                element.Text = string.Format("Best FPS: {0:#.00}", Root.Instance.BestFPS);

            element = OverlayManager.Instance.Elements.GetElement("Core/WorstFps");
            if (element != null)
                element.Text = string.Format("Worst FPS: {0:#.00}", Root.Instance.WorstFPS);

            //element = OverlayManager.Instance.Elements.GetElement( "Core/AverageFps" );
            //element.Text = string.Format( "Average FPS: {0:#.00}", Root.Instance.AverageFPS );
            element = OverlayManager.Instance.Elements.GetElement("Core/AverageFps");

            sum += Root.Instance.CurrentFPS;
            average = sum / elapsedFrames;
            elapsedFrames++;
            if (element != null)
                element.Text = string.Format("Average FPS: {0:#.00} in {1:#.0}s", average, (DateTime.Now - averageStart).TotalSeconds);

            element = OverlayManager.Instance.Elements.GetElement("Core/NumTris");
            if (element != null)
                element.Text = string.Format("Triangle Count: {0}", engine.Scene.TargetRenderSystem.FacesRendered);

            element = OverlayManager.Instance.Elements.GetElement("Core/NumBatches");
            if (element != null)
                element.Text = string.Format("Batch Count: {0}", engine.Scene.TargetRenderSystem.BatchesRendered);
        }
    }
}
