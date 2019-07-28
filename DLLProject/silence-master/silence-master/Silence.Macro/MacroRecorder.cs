using Silence.Hooking.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silence.Macro
{

    /// <summary>
    /// Allows recording sequences of mouse and keyboard events using global input hooks.
    /// </summary>
    public class MacroRecorder : IDisposable
    {

        /// <summary>
        /// Holds the underlying mouse/keyboard hook.
        /// </summary>
        private HookManager underlyingHook;

        /// <summary>
        /// Holds the time in ticks that the last event occurred.
        /// </summary>
        private long lastEventTime;

        /// <summary>
        /// Gets the macro currently being recorded.
        /// </summary>
        public Macro CurrentMacro { get; private set; }

        /// <summary>
        /// Gets whether or not the macro recorder is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        private bool FFIsCallbackfnSet = false;
        Action<MacroEvent> FFCallbackFn;
        Macro.MacroFlags FFCallBackFnEventType;

        /// <summary>
        /// Initialises a new instance of a macro recorder.
        /// </summary>
        public MacroRecorder()
        {
            underlyingHook = new HookManager();

            underlyingHook.KeyDown += underlyingHook_KeyDown;
            underlyingHook.KeyUp += underlyingHook_KeyUp;
            underlyingHook.MouseDown += underlyingHook_MouseDown;
            underlyingHook.MouseUp += underlyingHook_MouseUp;
            underlyingHook.MouseMove += underlyingHook_MouseMove;
            underlyingHook.MouseWheel += underlyingHook_MouseWheel;
        }

        public void SetCallbackEvent(Macro.MacroFlags f, Action<MacroEvent> fn)
        {
            FFIsCallbackfnSet = true;
            FFCallbackFn = fn;
            FFCallBackFnEventType = f;
        }

        /// <summary>
        /// Releases all resources associated with this object.
        /// </summary>
        public void Dispose()
        {
            underlyingHook.Dispose();
        }

        /// <summary>
        /// Adds a delay event to the current macro representing the time in ticks since this method was last called.
        /// </summary>
        private void AddDelayEvent()
        {
            long timeNow = DateTime.Now.Ticks;
            CurrentMacro.AddEvent(new MacroDelayEvent(timeNow - lastEventTime));
            lastEventTime = timeNow;
        }

        /// <summary>
        /// Loads a macro into the macro recorder.
        /// </summary>
        /// <param name="macro">The macro to load.</param>
        public void LoadMacro(Macro macro)
        {
            CurrentMacro = macro;
        }

        /// <summary>
        /// Clears the current macro from the macro recorder.
        /// </summary>
        public void Clear()
        {
            CurrentMacro = new Macro();
        }

        /// <summary>
        /// Starts recording mouse and keyboard events.
        /// </summary>
        public void StartRecording()
        {
            if (CurrentMacro == null)
            {
                Clear();
            }
            lastEventTime = DateTime.Now.Ticks;
            IsRunning = true;
        }

        /// <summary>
        /// Stops recording mouse and keyboard events.
        /// </summary>
        public void StopRecording()
        {
            IsRunning = false;
        }

        private void underlyingHook_KeyDown(object sender, Silence.Hooking.GlobalKeyEventHandlerArgs e)
        {
            
            if (IsRunning)
            {
                MacroKeyDownEvent ev = new MacroKeyDownEvent(e.VirtualKeyCode);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreKeyDownEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreKeyDownEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

        private void underlyingHook_KeyUp(object sender, Silence.Hooking.GlobalKeyEventHandlerArgs e)
        {
            if (IsRunning)
            {
                MacroKeyUpEvent ev = new MacroKeyUpEvent(e.VirtualKeyCode);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreKeyUpEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreKeyUpEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

        private void underlyingHook_MouseDown(object sender, Silence.Hooking.GlobalMouseEventHandlerArgs e)
        {
            if (IsRunning)
            {
                MacroMouseDownEvent ev = new MacroMouseDownEvent(e.Point, e.Button);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreMouseDownEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreMouseDownEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

        private void underlyingHook_MouseUp(object sender, Silence.Hooking.GlobalMouseEventHandlerArgs e)
        {
            if (IsRunning)
            {
                MacroMouseUpEvent ev = new MacroMouseUpEvent(e.Point, e.Button);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreMouseUpEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreMouseUpEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

        private void underlyingHook_MouseMove(object sender, Silence.Hooking.GlobalMouseEventHandlerArgs e)
        {
            if (IsRunning)
            {
                MacroMouseMoveEvent ev = new MacroMouseMoveEvent(e.Point);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreMouseMoveEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreMouseMoveEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

        private void underlyingHook_MouseWheel(object sender, Silence.Hooking.GlobalMouseEventHandlerArgs e)
        {
            if (IsRunning)
            {
                MacroMouseWheelEvent ev = new MacroMouseWheelEvent(e.Point, e.Delta);
                AddDelayEvent();
                CurrentMacro.AddEvent(ev);
                if (FFIsCallbackfnSet)
                {
                    if ((Macro.MacroFlags.IgnoreMouseWheelEvent & FFCallBackFnEventType) != Macro.MacroFlags.IgnoreMouseWheelEvent)
                    {
                        FFCallbackFn(ev);
                    }
                }
            }
        }

    }

}
