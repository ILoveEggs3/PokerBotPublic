using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsInputDLL
{
    public interface IMouseController
    {
        void MoveMouse(Point _from, Rectangle _to);
        Task MoveMouseAsync(Point _from, Rectangle _to);

        void MoveMouseFromCurrentLocation(Rectangle _to);
        Task MoveMouseFromCurrentLocationAsync(Rectangle _to);

        void MouseClickFromCurrentLocation();
    }
}
