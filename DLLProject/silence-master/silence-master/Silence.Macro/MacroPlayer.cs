using Silence.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Silence.Macro
{

    /// <summary>
    /// Provides support for playback of recorded macros.
    /// </summary>
    public class MacroPlayer
    {

        /// <summary>
        /// Holds the mouse simulator that underlies this object.
        /// </summary>
        private MouseSimulator underlyingMouseSimulator;

        /// <summary>
        /// Holds the keyboard simulator that underlies this object.
        /// </summary>
        private KeyboardSimulator underlyingKeyboardSimulator;

        /// <summary>
        /// Holds whether or not the macro has been cancelled.
        /// </summary>
        private bool cancelled;

        /// <summary>
        /// Holds the number of times the macro should repeat before ending.
        /// </summary>
        private int repetitions;

        /// <summary>
        /// Gets the macro currently loaded into the player.
        /// </summary>
        public Macro CurrentMacro { get; private set; }

        /// <summary>
        /// Gets or sets the number of times the macro should repeat before ending.
        /// </summary>
        public int Repetitions { 
            get 
            {
                return repetitions;
            }
            set
            {
                repetitions = (value == 0 ? 1 : value);
            }
        }

        /// <summary>
        /// Gets whether or not a macro is currently being played.
        /// </summary>
        public bool IsPlaying { get; private set; }

        private Random FFRnd;

        /// <summary>
        /// Initialies a new instance of a macro player.
        /// </summary>
        public MacroPlayer()
        {
            underlyingMouseSimulator = new MouseSimulator(new InputSimulator());
            underlyingKeyboardSimulator = new KeyboardSimulator(new InputSimulator());
            repetitions = 1;
            cancelled = false;
            FFRnd = new Random((int)DateTime.UtcNow.Ticks);
        }

        /// <summary>
        /// Cancels playback of the current macro.
        /// </summary>
        public void CancelPlayback()
        {
            cancelled = IsPlaying;
        }

        /// <summary>
        /// Loads a macro into the player.
        /// </summary>
        /// <param name="macro">The macro to load into the player.</param>
        public void LoadMacro(Macro macro) 
        {
            CurrentMacro = macro;
        }

        /// <summary>
        /// Converts a point to absolute screen coordinates.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns></returns>
        private Point ConvertPointToAbsolute(Point point)
        {
            return new Point((Convert.ToDouble(65535) * point.X) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Width),
                (Convert.ToDouble(65535) * point.Y) / Convert.ToDouble(Screen.PrimaryScreen.Bounds.Height));
        }


        private void PlayMacro(Point _from, Point _to, Macro.MacroFlags _flags, bool bIsFromToValid)
        {

            IsPlaying = true;
            Point initialCoord = new Point(Cursor.Position.X, Cursor.Position.Y);

            Point u = MathHelper.C2DTransformationHelper.PointsToVector(_from, _to);
            Point movEnd = CurrentMacro.PEndCoord;
            if (!bIsFromToValid)
            {
                _to = movEnd;
            }

            Point movBegin = CurrentMacro.PInitialCoord;
            if (!bIsFromToValid)
            {
                _from = movBegin;
            }

            Point v = MathHelper.C2DTransformationHelper.PointsToVector(movBegin, movEnd);
            double Ratio = MathHelper.C2DTransformationHelper.GetVectorLength(u) / MathHelper.C2DTransformationHelper.GetVectorLength(v);
            double angle = MathHelper.C2DTransformationHelper.GetAngleBetweenVectors(u, v);
            Point zeroU = new Point((initialCoord.X + _to.X) / 2, (initialCoord.Y + _to.Y) / 2);
            Point zeroV = new Point((movBegin.X + movEnd.X) / 2, (movBegin.Y + movEnd.Y) / 2);


            Point test = MathHelper.C2DTransformationHelper.Translate(initialCoord, new Point(-zeroU.X, -zeroU.Y));
            Point coord2 = new Point(movBegin.X, movBegin.Y);
            coord2 = MathHelper.C2DTransformationHelper.Translate(coord2, new Point(-zeroV.X, -zeroV.Y));
            coord2 = MathHelper.C2DTransformationHelper.Scale(coord2, Ratio);
            angle = MathHelper.C2DTransformationHelper.GetAngleBetweenVectors(coord2, test);

            var angle2 = -angle;
            var coord3 = MathHelper.C2DTransformationHelper.Rotate(coord2, angle);
            var coord4 = MathHelper.C2DTransformationHelper.Rotate(coord2, angle2);

            double diffC3 = MathHelper.C2DTransformationHelper.GetVectorLength(new Point(coord3.X - test.X, coord3.Y - test.Y));
            double diffC4 = MathHelper.C2DTransformationHelper.GetVectorLength(new Point(coord4.X - test.X, coord4.Y - test.Y));

            if (diffC3 > diffC4)
            {
                angle = angle2;
            }


            // Repeat macro as required.
            for (int i = 0; i < Repetitions; i++)
            {
                // Loop through each macro event.
                foreach (MacroEvent current in CurrentMacro.Events)
                {
                    // Cancel playback.
                    if (cancelled)
                    {
                        cancelled = false;
                        i = Repetitions;
                        break;
                    }

                    // Cast event to appropriate type.
                    if (((_flags & Macro.MacroFlags.IgnoreEventDelay) != Macro.MacroFlags.IgnoreEventDelay) && current is MacroDelayEvent)
                    {
                        // Delay event.
                        MacroDelayEvent castEvent = (MacroDelayEvent)current;
                        Thread.Sleep(new TimeSpan(FFRnd.Next((int)(0.6847d * castEvent.Delay), (int)((1.33492) * castEvent.Delay))));
                    }
                    else if (((_flags & Macro.MacroFlags.IgnoreMouseEvent) != Macro.MacroFlags.IgnoreMouseEvent) && current is MacroMouseEvent)
                    {
                        MacroMouseEvent castMouseEvent = (MacroMouseEvent)current;
                        Point coord = new Point(castMouseEvent.Location.X, castMouseEvent.Location.Y);
                        if (((_flags & Macro.MacroFlags.IgnoreMouseMoveEvent) != Macro.MacroFlags.IgnoreMouseMoveEvent))
                        {
                            coord = MathHelper.C2DTransformationHelper.Translate(coord, new Point(-zeroV.X, -zeroV.Y));
                            if (MathHelper.C2DTransformationHelper.GetVectorLength(v) > 0.5d)
                            {
                                coord = MathHelper.C2DTransformationHelper.Scale(coord, Ratio);
                                coord = MathHelper.C2DTransformationHelper.Rotate(coord, angle);
                            }
                            coord = MathHelper.C2DTransformationHelper.Translate(coord, zeroU);
                            Point absolutePoint = ConvertPointToAbsolute(coord);
                            underlyingMouseSimulator.MoveMouseTo(absolutePoint.X, absolutePoint.Y);
                        }

                        if (((_flags & Macro.MacroFlags.IgnoreMouseDownEvent) != Macro.MacroFlags.IgnoreMouseDownEvent) && current is MacroMouseDownEvent)
                        {
                            MacroMouseDownEvent castEvent = (MacroMouseDownEvent)current;
                            if (castEvent.Button == System.Windows.Input.MouseButton.Left)
                            {
                                underlyingMouseSimulator.LeftButtonDown();
                            }
                            else if (castEvent.Button == System.Windows.Input.MouseButton.Right)
                            {
                                underlyingMouseSimulator.RightButtonDown();
                            }
                        }
                        else if (((_flags & Macro.MacroFlags.IgnoreMouseUpEvent) != Macro.MacroFlags.IgnoreMouseUpEvent) && current is MacroMouseUpEvent)
                        {
                            MacroMouseUpEvent castEvent = (MacroMouseUpEvent)current;
                            if (castEvent.Button == System.Windows.Input.MouseButton.Left)
                            {
                                underlyingMouseSimulator.LeftButtonUp();
                            }
                            else if (castEvent.Button == System.Windows.Input.MouseButton.Right)
                            {
                                underlyingMouseSimulator.RightButtonUp();
                            }
                        }
                        else if (((_flags & Macro.MacroFlags.IgnoreMouseWheelEvent) != Macro.MacroFlags.IgnoreMouseWheelEvent) && current is MacroMouseWheelEvent)
                        {
                            MacroMouseWheelEvent castEvent = (MacroMouseWheelEvent)current;
                            underlyingMouseSimulator.VerticalScroll(castEvent.Delta / 120);
                        }
                    }
                    else if (((_flags & Macro.MacroFlags.IgnoreKeyEvent) != Macro.MacroFlags.IgnoreKeyEvent) && current is MacroKeyEvent)
                    {
                        if (current is MacroKeyDownEvent)
                        {
                            MacroKeyDownEvent castEvent = (MacroKeyDownEvent)current;
                            underlyingKeyboardSimulator.KeyDown((Simulation.Native.VirtualKeyCode)castEvent.VirtualKeyCode);
                        }
                        else if (current is MacroKeyUpEvent)
                        {
                            MacroKeyUpEvent castEvent = (MacroKeyUpEvent)current;
                            underlyingKeyboardSimulator.KeyUp((Simulation.Native.VirtualKeyCode)castEvent.VirtualKeyCode);
                        }
                    }
                }
            }

            IsPlaying = false;
        }

        public Task PlayMacroAsync(Point _from, Point _to, Macro.MacroFlags _flags = Macro.MacroFlags.NoIgnore)
        {
            return Task.Run(() =>
            {
                PlayMacro(_from, _to, _flags, true);
            });
        }

        public Task PlayMacroAsync(Macro.MacroFlags _flags = Macro.MacroFlags.NoIgnore)
        {
            return Task.Run(() =>
            {
                PlayMacro(new Point(), new Point(), _flags, false);
            });
        }
    }
}
