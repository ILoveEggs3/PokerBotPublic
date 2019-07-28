using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player
{

    public class CDummyPlayerController : CPlayerController
    {
        public CDummyPlayerController(IntPtr _launcherWindowHandle) : base(_launcherWindowHandle) { }

        public void DummyCheck(IntPtr _hwnd)
        {
            AddNewTables();
            if (_hwnd == IntPtr.Zero)
                if (FFTableReaderList.Count != 0)
                    _hwnd = FFTableReaderList.First().PHwnd;
            if (_hwnd != IntPtr.Zero)
            {
                Check(_hwnd);
            }
            else
            {
                Console.WriteLine("DummyPlayer: Nothing to do");
            }
        }

        public void DummyCheck()
        {
            DummyCheck(IntPtr.Zero);
        }

        public void DummyFold(IntPtr _hwnd)
        {
            AddNewTables();
            if (_hwnd == IntPtr.Zero)
                if (FFTableReaderList.Count != 0)
                    _hwnd = FFTableReaderList.First().PHwnd;
            if (_hwnd != IntPtr.Zero)
            {
                Fold(_hwnd);
            }
            else
            {
                Console.WriteLine("DummyPlayer: Nothing to do");
            }
        }

        public void DummyFold()
        {
            DummyFold(IntPtr.Zero);
        }


        public void DummyRaise(IntPtr _hwnd, decimal _value)
        {
            AddNewTables();
            if (_hwnd == IntPtr.Zero)
                if (FFTableReaderList.Count != 0)
                    _hwnd = FFTableReaderList.First().PHwnd;
            if (_hwnd != IntPtr.Zero)
            {
                Raise(_hwnd, _value);
            }
            else
            {
                Console.WriteLine("DummyPlayer: Nothing to do");
            }
        }

        public void DummyRaise(decimal _value)
        {
            DummyRaise(IntPtr.Zero, _value);
        }
    }
}
