using Silence.Macro;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmigoDB
{
    public sealed class CMouseMovement
    {
        public int PXCoordsBegin { set; get; }
        public int PYCoordsBegin { set; get; }
        public int PXCoordsEnd { set; get; }
        public int PYCoordsEnd { set; get; }
        public double PTrueDistance { get; set; } // True Distance of the movement
        public double PEuclideDistance { get; set; } // Distance of the straight line from A to B
        public double POrientation { get; set; }
        public bool PClickAtTheEnd { get; set; } // does the move contains a click at the end
        public ulong PTicks { set; get; }
        public string PMovements { set; get; }
        public CMouseMovement(int bX, int bY, int eX, int eY, double trueDist, double euclideDist, double orientation, bool clickAtTheEnd, ulong ticks, string xmlStr)
        {
            PXCoordsBegin = bX;
            PYCoordsBegin = bY;

            PXCoordsEnd = eX;
            PYCoordsEnd = eY;

            PTrueDistance = trueDist;
            PEuclideDistance = euclideDist;
            POrientation = orientation;
            PClickAtTheEnd = clickAtTheEnd;
            PTicks = ticks;

            PMovements = xmlStr;
        }
        public CMouseMovement(Macro _macro)
        {
            PXCoordsBegin = (int)_macro.PInitialCoord.X;
            PYCoordsBegin = (int)_macro.PInitialCoord.Y;

            PXCoordsEnd = (int)_macro.PEndCoord.X;
            PYCoordsEnd = (int)_macro.PEndCoord.Y;

            PTicks = (ulong)_macro.PTicks;

            PMovements = _macro.ToXml();
        }

        public Macro GetMacro()
        {
            Macro mac = new Macro();
            mac.LoadMacroFromXmlString(PMovements);
            return mac;
        }
    }
}
