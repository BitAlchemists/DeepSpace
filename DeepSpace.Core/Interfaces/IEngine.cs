using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpace.Core.Interfaces
{
    public delegate void FrameStartedHandler(float dT);
    public delegate void FrameEndedHandler();

    public interface IEngine
    {
        FrameStartedHandler FrameStarted { get; set; }
        FrameEndedHandler FrameEnded { get; set; }
        
        List<IEngineSubsystem> Subsystems { get; }

        bool Setup();
        void StartRendering();
        void StopRendering();
        void AddSubsystem(IEngineSubsystem subsystem);
    }
}
