using AmigoDB;
using Silence.Macro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsInputDLL
{
    public class CInputControllerSilence : IMouseController, IKeyboardController
    {
        private MacroPlayer FFMacroPlayer;
        private Random FFRnd;

        public CInputControllerSilence()
        {
            FFMacroPlayer = new MacroPlayer();
            FFRnd = new Random((int)(DateTime.UtcNow.Ticks));
        }

        private Point GetRandomPointFromRectangle(Rectangle _rec)
        {
            return new Point(FFRnd.Next(_rec.X, _rec.X + _rec.Width), FFRnd.Next(_rec.Y, _rec.Y + _rec.Height));
        }

        private void LoadRandomMovementFromDB(Point _from, Point _to, Size _timeBoundaries)
        {

            Point V = new Point(_to.X - _from.X, _to.Y - _from.Y);
            List<CMouseMovement> movementList = CDBHelper.GetMouseMovements();
            int nbToChoose = (int)Math.Sqrt(movementList.Count);
            double fract = Math.Sqrt((1.0d * nbToChoose / movementList.Count));
            if (fract > 0.99999)
            {
                fract = 0.99999;
            }
            if (fract <  0.01)
            {
                fract = 0.01;
            }
            movementList.Sort(delegate (CMouseMovement f, CMouseMovement l) // get the movements that the distances are the closest to the actual one
            {
                double d = MathHelper.C2DTransformationHelper.GetVectorLength(new System.Windows.Point(V.X, V.Y));
                double diff1 = Math.Abs(f.PEuclideDistance - d);
                double diff2 = Math.Abs(l.PEuclideDistance - d);
                if (diff1 < diff2)
                {
                    return -1;
                }
                return 1;
            });
            int thresholdInd = (int)(fract * movementList.Count);
            movementList.RemoveRange(thresholdInd, movementList.Count - thresholdInd);

            movementList.Sort(delegate (CMouseMovement f, CMouseMovement l) // get the movements that the distances are the closest to the actual one
            {
                double d = MathHelper.C2DTransformationHelper.GetAngleBetweenVectors(new System.Windows.Point(V.X, V.Y), new System.Windows.Point(10.0d, 0.0d));
                if (V.Y < 0)
                {
                    d = -d;
                    d = Math.PI * 2 + d;
                }
                double d1 = f.POrientation < 0 ? Math.PI * 2 + f.POrientation : f.POrientation;
                double d2 = l.POrientation < 0 ? Math.PI * 2 + l.POrientation : l.POrientation;

                double diff1 = Math.Abs(d1 - d);
                double diff2 = Math.Abs(d2 - d);
                if (diff1 < diff2)
                {
                    return -1;
                }
                return 1;
            });
            thresholdInd = (int)(fract * movementList.Count);
            movementList.RemoveRange(thresholdInd, movementList.Count - thresholdInd);

            if (movementList.Count == 0)
                return;

            movementList.Sort(delegate (CMouseMovement f, CMouseMovement l)
            {
                if (f.PTrueDistance < l.PTrueDistance)
                {
                    return -1;
                }
                return 1;
            });
            int randInd = FFRnd.Next(0, movementList.Count/2+1);

            Macro loadedMacro = new Macro();
            randInd = FFRnd.Next(0, movementList.Count);
            loadedMacro.LoadMacroFromXmlString(movementList[randInd].PMovements);

            double ratio = 1;

            if (loadedMacro.PTicks < (_timeBoundaries.Width * 10000))
            {
                ratio = (double)(_timeBoundaries.Width * 10000) / loadedMacro.PTicks;
            }
            else if (loadedMacro.PTicks > (_timeBoundaries.Height * 10000))
            {
                ratio = (double)(_timeBoundaries.Height * 10000) / loadedMacro.PTicks;
            }
            for (int i = 0; i < loadedMacro.Events.Length; i++)
            {
                var ev = loadedMacro.Events[i];
            
                if (ev is MacroDelayEvent)
                {
                    var e = (MacroDelayEvent)ev;
                    e.Delay = (long)(e.Delay * ratio);
                }
            }

        FFMacroPlayer.LoadMacro(loadedMacro);
        }

        private void LoadRandomMovementFromDB(Point _from, Point _to)
        {
            LoadRandomMovementFromDB(_from, _to, new Size(0, 100000 * 5));
        }

        public void MouseClickFromCurrentLocation()
        {
            Macro clickMacro = new Macro();
            clickMacro.AddEvent(new MacroMouseDownEvent(new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y), System.Windows.Input.MouseButton.Left));
            clickMacro.AddEvent(new MacroDelayEvent(new TimeSpan(FFRnd.Next(559522 / 2, 2 * 559522)).Ticks));
            clickMacro.AddEvent(new MacroMouseUpEvent(new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y), System.Windows.Input.MouseButton.Left));
            FFMacroPlayer.LoadMacro(clickMacro);
            FFMacroPlayer.PlayMacroAsync(Macro.MacroFlags.IgnoreMouseMoveEvent).Wait();
        }

        public Task MouseClickFromCurrentLocationAsync()
        {
            Macro clickMacro = new Macro();
            clickMacro.AddEvent(new MacroMouseDownEvent(new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y), System.Windows.Input.MouseButton.Left));
            clickMacro.AddEvent(new MacroDelayEvent(new TimeSpan(FFRnd.Next(559522 / 2, 2 * 559522)).Ticks));
            clickMacro.AddEvent(new MacroMouseUpEvent(new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y), System.Windows.Input.MouseButton.Left));
            FFMacroPlayer.LoadMacro(clickMacro);
            return FFMacroPlayer.PlayMacroAsync();
        }

        public void MoveMouse(Point _from, Rectangle _to)
        {
            MoveMouseAsync(_from, _to).Wait();
        }

        public Task MoveMouseAsync(Point _from, Rectangle _to)
        {
            //LoadRandomMovement();
            Point to = GetRandomPointFromRectangle(_to);
            LoadRandomMovementFromDB(_from, to);
            Macro.MacroFlags flags =
                Macro.MacroFlags.IgnoreKeyEvent |
                Macro.MacroFlags.IgnoreMouseWheelEvent |
                Macro.MacroFlags.IgnoreMouseDownEvent |
                Macro.MacroFlags.IgnoreMouseUpEvent;
            return FFMacroPlayer.PlayMacroAsync(new System.Windows.Point(_from.X, _from.Y), new System.Windows.Point(to.X, to.Y), flags);
        }
        public Task MoveMouseAsync(Point _from, Rectangle _to, Size _timeBoundaries)
        {
            //LoadRandomMovement();
            Point to = GetRandomPointFromRectangle(_to);
            LoadRandomMovementFromDB(_from, to, _timeBoundaries);
            Macro.MacroFlags flags =
                Macro.MacroFlags.IgnoreKeyEvent |
                Macro.MacroFlags.IgnoreMouseWheelEvent |
                Macro.MacroFlags.IgnoreMouseDownEvent |
                Macro.MacroFlags.IgnoreMouseUpEvent;
            return FFMacroPlayer.PlayMacroAsync(new System.Windows.Point(_from.X, _from.Y), new System.Windows.Point(to.X, to.Y), flags);
        }

        public void MoveMouseFromCurrentLocation(Rectangle _to)
        {
            MoveMouseFromCurrentLocationAsync(_to).Wait();
        }

        public void MoveMouseFromCurrentLocation(Rectangle _to, bool _bClickAtTheEnd = false)
        {
            MoveMouseFromCurrentLocationAsync(_to).Wait();
            if (_bClickAtTheEnd)
            {
                MouseClickFromCurrentLocation();
            }
        }

        public void MoveMouseFromCurrentLocation(Rectangle _to, Size _timeBoundaries, bool _bClickAtTheEnd = false)
        {
            MoveMouseFromCurrentLocationAsync(_to, _timeBoundaries).Wait();
            if (_bClickAtTheEnd)
            {
                MouseClickFromCurrentLocation();
            }
        }

        public Task MoveMouseFromCurrentLocationAsync(Rectangle _to)
        {
            return MoveMouseAsync(Cursor.Position, _to);
        }

        public Task MoveMouseFromCurrentLocationAsync(Rectangle _to, Size _timeBoundaries)
        {
            return MoveMouseAsync(Cursor.Position, _to, _timeBoundaries);
        }

        public void InputString(string _str)
        {
            InputStringAsync(_str).Wait();
        }

        public Task InputStringAsync(string _str)
        {
            Macro inpuStringMacro = new Macro();
            foreach (var c in _str)
            {
                inpuStringMacro.AddEvent(new MacroDelayEvent(new TimeSpan(FFRnd.Next(523452, (int)(523452 * 5.27))).Ticks));
                inpuStringMacro.AddEvent(new MacroKeyDownEvent((int)Keyboard.CharToVirtualKeyShort[c]));
                inpuStringMacro.AddEvent(new MacroDelayEvent(new TimeSpan(FFRnd.Next(332342, (int)(332342 * 1.64))).Ticks));
                inpuStringMacro.AddEvent(new MacroKeyUpEvent((int)Keyboard.CharToVirtualKeyShort[c]));
            }
            inpuStringMacro.AddEvent(new MacroDelayEvent(new TimeSpan(FFRnd.Next((int)(523452 * 5.27), (int)(523452 * 5.27 * 3))).Ticks));
            FFMacroPlayer.LoadMacro(inpuStringMacro);
            return FFMacroPlayer.PlayMacroAsync(Macro.MacroFlags.IgnoreMouseEvent);
        }
    }
}
