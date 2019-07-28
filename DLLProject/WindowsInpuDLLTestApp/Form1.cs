using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WindowsInputDLL;
using Resources;
using System.IO;
using System.Diagnostics;
using ScreenScraping.ScreenCapture;
using ScreenScraping.Readers.Events;
using ScreenScraping.Readers.TableReader.TwoMax;
using OpenCL;
using Player;

namespace WindowsInpuDLLTestApp
{
    public partial class Form1 : Form
    {
        //private readonly MacroRecorder FFRecorder = new MacroRecorder();
        CInputControllerSilence FFInputController = new CInputControllerSilence();
        Random FFRnd = new Random((int)DateTime.UtcNow.Ticks);
        private ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController tableController = null;
        CConstantsTable constants = new Resources.TwoMax.EspaceJeux.Constantes();
        CReferences references = new Resources.TwoMax.EspaceJeux.References();
        private IntPtr FFHandle = IntPtr.Zero;
        public Form1()
        {
            InitializeComponent();

            /*if (tableController == null)
            {
                var hwndList = WindowHelper.GetHandlePtrFromMainWindowTitle("Table");
                FFHandle = hwndList.First();
                WindowHelper.SetWindowRegion(hwndList.First(), constants.TABLE_DIMENSION.Width, constants.TABLE_DIMENSION.Height);
                tableController = new CController(hwndList.First(), ScreenScraping.ScreenCapture.PInstance);
            }*/
        }

        private static Bitmap CaptureImage(int x, int y)
        {
            Bitmap b = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(100, 100), CopyPixelOperation.SourceCopy);
                g.DrawLine(Pens.Black, new Point(0, 27), new Point(99, 27));
                g.DrawLine(Pens.Black, new Point(0, 73), new Point(99, 73));
                g.DrawLine(Pens.Black, new Point(52, 0), new Point(52, 99));
                g.DrawLine(Pens.Black, new Point(14, 0), new Point(14, 99));
                g.DrawLine(Pens.Black, new Point(85, 0), new Point(85, 99));
            }
            return b;
        }

        public void RandomMove()
        {
            Thread.Sleep(10000);
            int i = 0;
            while (i++ < 1) 
            {
                int X = FFRnd.Next(100, 1000);
                int Y = FFRnd.Next(100, 1000);
                FFInputController.MoveMouseFromCurrentLocation(new Rectangle(X, Y, 1, 1));
                FFInputController.MouseClickFromCurrentLocation();
                Thread.Sleep(500);
            }
        }

        public void TestAllSamples()
        {
            var sampleDirectoryPath = @"C:\Users\dinf0014_admin\Desktop\ReferenceBMP\Samples2";
            var fileArray = Directory.GetFiles(sampleDirectoryPath);
            tableController = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(IntPtr.Zero, DummyScreenCapture.PInstance);
            foreach (var file in fileArray)
            {
                using (var bmp = new Bitmap(file))
                {
                    DummyScreenCapture.PInstance.SetBitmap(bmp);
                    //tableController.IsOurTurn();
                    //tableController.GetOurHand();
                    tableController.GetFlop();
                    tableController.GetTurn();
                    tableController.GetRiver();
                }
            }
        }

        public void TakeScreenShot()
        {

            var hwndList = WindowHelper.GetHandlePtrFromMainWindowTitle("Table");
            FFHandle = hwndList.First();
            WindowHelper.SetWindowRegion(hwndList.First(), constants.WINDOW_DIMENSION.Width, constants.WINDOW_DIMENSION.Height);
            tableController = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(hwndList.First(), ScreenCapture.PInstance);


            var qwe = WindowHelper.CaptureWindowFromHandle(FFHandle);
            qwe.Save(@"C:\Users\dinf0014_admin\Desktop\ReferenceBMP\Samples4\sample" + DateTime.UtcNow.Ticks.ToString()+".bmp");
        }



        public void Test()
        {
            var processes = Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains("Poker | EspaceJeux".ToLower()) && !x.MainWindowTitle.ToLower().Contains("Table".ToLower()));
            List<IntPtr> ptrList = new List<IntPtr>();
            foreach (var p in processes)
            {
                ptrList.Add(p.MainWindowHandle);
            }

            FFHandle = ptrList.First();

            var c = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(FFHandle, ScreenCapture.PInstance);

            var launcherConstants = new Resources.Launcher.EspaceJeux.Constantes();

            ScreenScraping.ScreenCapture.WindowHelper.SetWindowRegion(FFHandle, launcherConstants.WINDOW_DIMENSION.Width, launcherConstants.WINDOW_DIMENSION.Height, 0, 0, WindowHelper.WindowsFlags.SWP_SHOWWINDOW | WindowHelper.WindowsFlags.SWP_NOREPOSITION | WindowHelper.WindowsFlags.SWP_NOZORDER | WindowHelper.WindowsFlags.SWP_NOMOVE);
            //var bmp = ScreenCapture.PInstance.GetScreenshot(FFHandle);
            //bmp.Save(@"C:\Users\admin\Desktop\ReferenceBMP\LauncherSamples\" + DateTime.UtcNow.Ticks.ToString() + ".bmp");

            Player.CPlayerController player = new Player.CPlayerController(FFHandle);



            player.SetFilters();
            player.MonitorFullTablesAndPopup();
            //player.AddNewTables();
            //player.PositionTables();

            //Thread.Sleep(1000);
            //player.CloseAllTables();
            /*DummyScreenCapture.PInstance.SetBitmap(new Bitmap(@"C:\Users\admin\Desktop\ReferenceBMP\Samples4\sample636723966834322730.bmp"));
            tableController = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(IntPtr.Zero, DummyScreenCapture.PInstance);
            var qwe = tableController.GetSeatedPlayers();*/

           /* tableController.EOurTurn += OnOurTurn;
            tableController.StartMonitoring();*/

            /*foreach (var item in Directory.GetFiles(@"C:\Users\admin\Desktop\ReferenceBMP\Samples2\"))
            {
                DummyScreenCapture.PInstance.SetBitmap(new Bitmap(item));
                var qwe = tableController.GetPlayerStack(CTableReaderTwoMax.PlayerPosition.P0);
                Console.WriteLine("Stack P0: {0}", qwe);
                qwe = tableController.GetPlayerStack(CTableReaderTwoMax.PlayerPosition.P1);
                Console.WriteLine("Stack P1: {0}", qwe);
            }*/



            //var bet = tableController.GetTotalPot();
            //var qwe = tableController.GetFlop();
            


            /*tableController.StartMonitoring();
            tableController.PMonitoringWaitTimeMs = 1000;

            tableController.NewRiverCard += OnRiver;
            tableController.OurTurn += OnOurTurn;*/

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //test
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //TestAllSamples();
            Test();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            //Console.WriteLine("Time Take: {0}ms", elapsedMs);
        }

        public bool IsOurTurn()
        {
            if (tableController == null)
            {
                var hwndList = WindowHelper.GetHandlePtrFromMainWindowTitle("Table");
                FFHandle = hwndList.First();
                WindowHelper.SetWindowRegion(hwndList.First(), constants.WINDOW_DIMENSION.Width, constants.WINDOW_DIMENSION.Height);
                tableController = new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(hwndList.First(), ScreenCapture.PInstance);
            }

            return tableController.IsOurTurn();
        }

        public void OnRiver(CTableReaderTwoMax sender, RiverEventArgs e)
        {
            Console.WriteLine("New River Card Notified!");
            bool qwe = sender == tableController;
            //tableController.StopMonitoring();
        }

        public void OnOurTurn(CTableReaderTwoMax sender, OurTurnEventArgs e)
        {
            Console.WriteLine("On Our Turn Notified!");
        }

        private void cmd_Check_Click(object sender, EventArgs e)
        {
            var qwe = new CDummyPlayerController(IntPtr.Zero);

            qwe.DummyCheck();


        }
    }
}
