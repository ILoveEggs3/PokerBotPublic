using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenScraping.ScreenCapture
{
    public class WindowHelper
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError =true)]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int wFlags);

        [Flags]
        public enum WindowsFlags
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
            SWP_DEFERERASE = 0x2000,
            SWP_DRAWFRAME = 0x0020,
            SWP_FRAMECHANGED = 0x0020,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOACTIVATE = 0x0010,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOMOVE = 0x0002,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOREDRAW = 0x0008,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSENDCHANGING = 0x0400,
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_SHOWWINDOW = 0x0040
        }

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public static List<IntPtr> GetHandlePtrFromMainWindowTitle(string _mainWindowTitle)
        {
            var qwe = Process.GetProcesses();
            var processes = Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains(_mainWindowTitle.ToLower()));
            List<IntPtr> ptrList = new List<IntPtr>();
            foreach (var p in processes)
            {
                ptrList.Add(p.MainWindowHandle);
            }
            return ptrList;
        }

        public static List<Process> GetProcessListFromMainWindowTitle(string _mainWindowTitle)
        {
            var qwe = Process.GetProcesses();
            List<Process> processes = Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains(_mainWindowTitle.ToLower())).ToList();
            return processes;
        }

        public static Bitmap CaptureWindowFromHandle(IntPtr handle)
        {
            return ScreenCapture.PInstance.GetScreenshot(handle);
        }

        public static Bitmap CaptureWindowFromRegion(Rectangle rect)
        {
            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            }

            return result;
        }

        public static void SetWindowRegion(IntPtr handle, int cx, int cy, int X = 0, int Y = 0, WindowsFlags wflags = (WindowsFlags.SWP_NOZORDER | WindowsFlags.SWP_SHOWWINDOW))
        {
            if (handle != IntPtr.Zero)
            {
                var qwe = SetWindowPos(handle, 0, X, Y, cx, cy, (int)wflags);
            }
        }
    }
}
