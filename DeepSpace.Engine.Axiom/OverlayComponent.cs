using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core;
using Axiom.Overlays;
using DeepSpace.Core.Logging;

namespace DeepSpace.Engine.Axiom
{
    public class OverlayComponent : GameObjectComponent
    {
        public virtual string Name { get; set; }
        bool showing;

        public OverlayComponent()
        {
            showing = false;
        }


        /// <summary>
        ///    Shows the debug overlay, which displays performance statistics.
        /// </summary>

        public void ShowOverlay(bool show)
        {
            // gets a reference to the default overlay
            Overlay o = OverlayManager.Instance.GetByName(Name);

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

            showing = show;
        }

        public void ToggleOverlay()
        {
            ShowOverlay(!showing);
        }
    }
}
