using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeepSpace.Core;
using Axiom.Input;
using DeepSpace.Core.Logging;

namespace DeepSpace.Engine.Axiom
{
    public class InputComponent : GameObjectComponent
    {
        class ToggleableInputHandler
        {
            public InputHandler keyHandler;
            public float startToggleDelay;
            public float toggleDelay;
        }

        public delegate void InputHandler();
        Dictionary<KeyCodes, InputHandler> keyHandlers;
        Dictionary<KeyCodes, ToggleableInputHandler> toggleableKeyHandlers;
        internal InputReader Input { get; set; }


        public InputComponent()
        {
            keyHandlers = new Dictionary<KeyCodes, InputHandler>();
            toggleableKeyHandlers = new Dictionary<KeyCodes, ToggleableInputHandler>();
        }

        public void AddKeyDelegate(KeyCodes keyCode, InputHandler keyHandler)
        {
            keyHandlers.Add(keyCode, keyHandler);
        }

        public void AddToggleableKeyDelegate(KeyCodes keyCode, InputHandler keyHandler)
        {
            AddToggleableKeyDelegate(keyCode, keyHandler, 0.5f);
        }

        public void AddToggleableKeyDelegate(KeyCodes keyCode, InputHandler keyHandler, float toggleDelay)
        {
            toggleableKeyHandlers.Add(keyCode, new ToggleableInputHandler() { keyHandler = keyHandler, startToggleDelay = toggleDelay, toggleDelay = 0.0f });
        }

        public override void Update(float dT)
        {
            foreach (KeyCodes keyCode in keyHandlers.Keys)
            {
                if (Input.IsKeyPressed(keyCode))
                {
                    InputHandler keyHandler;
                    if(keyHandlers.TryGetValue(keyCode, out keyHandler))
                    {
                        keyHandler();
                    }
                }
            }

            foreach (KeyCodes keyCode in toggleableKeyHandlers.Keys)
            {
                //fetch the value for the key
                ToggleableInputHandler toggleableKeyHandler;
                toggleableKeyHandlers.TryGetValue(keyCode, out toggleableKeyHandler);

                //reduce the toggle delay
                toggleableKeyHandler.toggleDelay -= dT;

                //check if we have to do anything
                if (Input.IsKeyPressed(keyCode))
                {
                    if (toggleableKeyHandler.toggleDelay <= 0)
                    {
                        toggleableKeyHandler.keyHandler();
                        toggleableKeyHandler.toggleDelay = toggleableKeyHandler.startToggleDelay;
                    }

                }
            }
        }
    }
}
