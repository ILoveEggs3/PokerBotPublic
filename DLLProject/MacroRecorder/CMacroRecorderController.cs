using Silence.Macro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recorder
{
    public class CMacroRecorderController
    {
        private readonly MacroRecorder FFRecorder = new MacroRecorder();
        Action<Macro> FFCallbackFunction;

        public CMacroRecorderController(Action<Macro> _callBackFunction)
        { 
            FFCallbackFunction = _callBackFunction;
        }

        public void Start()
        {
            FFRecorder.Clear();
            FFRecorder.SetCallbackEvent((~Macro.MacroFlags.NoIgnore)^Macro.MacroFlags.IgnoreMouseUpEvent, stopEvent); // ignore all events execpt MouseUpEvent for the callback
            FFRecorder.StartRecording();
        }

        public void Stop()
        {
            FFRecorder.StopRecording();
        }

        private void stopEvent(MacroEvent e)
        { 
            FFRecorder.StopRecording();
            if (FFRecorder.CurrentMacro.PTicks < 25000000 && FFRecorder.CurrentMacro.PTrueDistance > 1.01)
                FFCallbackFunction(FFRecorder.CurrentMacro);
            FFRecorder.Clear();
            FFRecorder.StartRecording();
        }
    }
}
