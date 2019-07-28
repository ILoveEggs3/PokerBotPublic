using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Xml;

namespace Silence.Macro
{

    /// <summary>
    /// Represents a recorded sequence of mouse and keyboard events.
    /// </summary>
    public class Macro
    {

        [Flags]
        public enum MacroFlags
        {
            NoIgnore = 0,
            IgnoreEventDelay = 0x1 << 0,
            IgnoreMouseEvent = IgnoreMouseDownEvent | IgnoreMouseMoveEvent | IgnoreMouseUpEvent | IgnoreMouseWheelEvent,
            IgnoreKeyEvent = IgnoreKeyDownEvent | IgnoreKeyUpEvent,
            IgnoreKeyDownEvent = 0x1 << 3,
            IgnoreKeyUpEvent = 0x1 << 4,
            IgnoreMouseDownEvent = 0x1 << 5,
            IgnoreMouseMoveEvent = 0x1 << 6,
            IgnoreMouseUpEvent = 0x1 << 7,
            IgnoreMouseWheelEvent = 0x1 << 8
        }
        /// <summary>
        /// Holds the list of events that comprise this macro.
        /// </summary>
        private List<MacroEvent> events;

        public Point PInitialCoord {
            get
            {
                MacroMouseEvent e;
                int ind = -1;
                while (++ind < Events.Length && !(Events[ind] is MacroMouseEvent)) ;
                if (ind == Events.Length)
                {
                    return new Point(-1, -1);
                }
                else
                {
                    e = (MacroMouseEvent)Events[ind];
                    return e.Location;
                }
            }
        }

        public Point PEndCoord
        {
            get
            {
                MacroMouseEvent e;
                int ind = Events.Length;
                while (--ind >= 0 && !(Events[ind] is MacroMouseEvent)) ;
                if (ind < 0)
                {
                    return new Point(-1, -1);
                }
                else
                {
                    e = (MacroMouseEvent)Events[ind];
                    return e.Location;
                }
            }
        }

        public double PTrueDistance
        {
            get
            {
                double dist = 0;
                Point coord = PInitialCoord;
                foreach (var item in events)
                {
                    if (item is MacroMouseEvent)
                    {
                        MacroMouseEvent ev = (MacroMouseEvent)item;
                        dist += MathHelper.C2DTransformationHelper.GetVectorLength(new Point(coord.X - ev.Location.X, coord.Y - ev.Location.Y));
                        coord = ev.Location;
                    }
                }
                return dist;
            }
        }

        public double PEuclideDist
        {
            get
            {
                return MathHelper.C2DTransformationHelper.GetVectorLength(new Point(PInitialCoord.X - PEndCoord.X, PInitialCoord.Y - PEndCoord.Y));
            }
        }

        public double POrientation
        {
            get
            {
                Point v = new Point(PEndCoord.X - PInitialCoord.X, PEndCoord.Y - PInitialCoord.Y);
                Point xV = new Point(10.0d, 0.0d);
                double orientation = MathHelper.C2DTransformationHelper.GetAngleBetweenVectors(v, xV);
                return v.Y < 0.0d ? -orientation : orientation;
            }
        }

        public bool PClickAtTheEnd
        {
            get
            {
                MacroMouseEvent e;
                int ind = Events.Length;
                while (--ind >= 0 && !(Events[ind] is MacroMouseUpEvent)) ;
                if (ind == Events.Length)
                {
                    return false; //no mouse click found
                }
                else
                {
                    e = (MacroMouseUpEvent)Events[ind];
                    while (--ind >= 0 && !(Events[ind] is MacroMouseDownEvent)) ;
                    if (ind == Events.Length)
                    {
                        return false; // no mouse down situation (is very weird)
                    }
                    MacroMouseDownEvent d = (MacroMouseDownEvent)Events[ind];
                    if (MathHelper.C2DTransformationHelper.GetVectorLength(new Point(e.Location.X - d.Location.X, e.Location.Y - d.Location.Y)) > 2)
                    {
                        return false; // drag and drop situation
                    }
                    if (MathHelper.C2DTransformationHelper.GetVectorLength(new Point(e.Location.X - PEndCoord.X, e.Location.Y - PEndCoord.Y)) > 3)
                    {
                        return false; // not at the end situation
                    }
                    return true;
                }
            }
        }

        public long PTicks
        {
            get
            {
                long ticks = 0;
                MacroDelayEvent e;
                foreach (var item in Events)
                {
                    if (item is MacroDelayEvent)
                    {
                        e = (MacroDelayEvent)item;
                        ticks += e.Delay;
                    }
                }
                return ticks;
            }
        }

        /// <summary>
        /// Gets the list of events that comprise this macro as an array.
        /// </summary>
        public MacroEvent[] Events {
            get
            {
                return (events == null ? null : events.ToArray());
            }
        }

        /// <summary>
        /// Initialises a new instance of a macro.
        /// </summary>
        public Macro()
        {
            events = new List<MacroEvent>();
        }

        private Macro(List<MacroEvent> _macroList)
        {
            events = _macroList;
        }

        public Macro RemoveFirstCoord()
        {
            Macro temp = new Macro(events);

            MacroMouseEvent e;
            Point _initialCoord = temp.PInitialCoord;

            int i = 0;
            for (; i < temp.events.Count; i++)
            {
                if (temp.events[i] is MacroMouseEvent)
                {
                    e = (MacroMouseEvent)temp.events[i];
                    if (e.Location.X != _initialCoord.X && e.Location.Y != _initialCoord.Y)
                    {
                        break;
                    }
                }
            }
            temp.events.RemoveRange(0, i - 1);
            return temp;
        }

        /// <summary>
        /// Appends an event to the end of this macro.
        /// </summary>
        /// <param name="newEvent">The event to append.</param>
        public void AddEvent(MacroEvent newEvent)
        {
            events.Add(newEvent);
        }

        /// <summary>
        /// Removes all events from this macro.
        /// </summary>
        public void ClearEvents()
        {
            events.Clear();
        }

        /// <summary>
        /// Serialises this object to an XML string for saving.
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("<SilenceMacro>");
            foreach (MacroEvent current in events)
            {
                str.AppendLine(current.ToXml());
            }
            str.AppendLine("</SilenceMacro>");

            return str.ToString();
        }

        /// <summary>
        /// Saves this macro to a file at the specified path.
        /// </summary>
        /// <param name="path">The path to save to.</param>
        public void Save(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(ToXml());

            xmlDoc.Save(path);
        }


        private void LoadFromXMLDocument(XmlDocument xmlDoc)
        {
            foreach (XmlElement current in xmlDoc.DocumentElement)
            {
                switch (current.Name)
                {
                    case "MacroKeyDownEvent":
                        AddEvent(new MacroKeyDownEvent(current));
                        break;
                    case "MacroKeyUpEvent":
                        AddEvent(new MacroKeyUpEvent(current));
                        break;
                    case "MacroMouseDownEvent":
                        AddEvent(new MacroMouseDownEvent(current));
                        break;
                    case "MacroMouseUpEvent":
                        AddEvent(new MacroMouseUpEvent(current));
                        break;
                    case "MacroMouseMoveEvent":
                        AddEvent(new MacroMouseMoveEvent(current));
                        break;
                    case "MacroMouseWheelEvent":
                        AddEvent(new MacroMouseWheelEvent(current));
                        break;
                    case "MacroDelayEvent":
                        AddEvent(new MacroDelayEvent(current));
                        break;
                }
            }
        }

        public void LoadFromFile(string path)
        {
            ClearEvents();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            LoadFromXMLDocument(xmlDoc);
        }

        public void LoadMacroFromXmlString(string xml)
        {
            Macro m = new Macro();
            m.ClearEvents();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            LoadFromXMLDocument(xmlDoc);
        }

    }

}
