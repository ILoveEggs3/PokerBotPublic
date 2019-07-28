using Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScreenScraping;
using ScreenScraping.ScreenCapture;
using System.Threading;

namespace ScreenScraping.Readers
{
    public abstract class CAbstractReader
    {
        protected delegate void MonitoringFunction(Bitmap _bmp);
        protected List<MonitoringFunction> FFMonitoringFunctionList;
        protected const string FFInaccurateResultsFolderPath = @"C:\Users\admin\Desktop\ReferenceBMP\InaccurateRead\";

        public IntPtr PHwnd { get; }
        public Bitmap PCurrentBmp { get; private set; }
        public int PMonitoringWaitTimeMs { get; set; }

        private readonly IScreenShotHelper FFScreenShotHelper;
        private bool FFIsMonitoring { get; set; }
        private Task FFMonitoringTask { get; set; }


        protected CAbstractReader(
            IntPtr _hwnd,
            IScreenShotHelper _screenShotHelper)
        {
            PHwnd = _hwnd;
            FFScreenShotHelper = _screenShotHelper;

            PMonitoringWaitTimeMs = 10000;
            FFMonitoringFunctionList = new List<MonitoringFunction>();
        }

        public override bool Equals(object obj)
        {
            if (obj is CAbstractReader)
                return this.PHwnd == ((CAbstractReader)obj).PHwnd;
            if (obj is IntPtr)
                return this.PHwnd == (IntPtr)obj;
            return base.Equals(obj);
        }

        public void UpdateCurrentImage()
        {
            PCurrentBmp = FFScreenShotHelper.GetScreenshot(PHwnd);
        }

        protected decimal GetNumbers(Bitmap _bmp, List<Point> _sampleCoordList, List<Tuple<char, Bitmap>> _referenceList, Rectangle _numbersDimension)
        {
            var ret = OpenCL.OpenCLController.CalculateDistances(_bmp, _sampleCoordList, _referenceList.ConvertAll(x => x.Item2));
            string number = "";
            int indDot = 0;
            bool dotFound = false;
            for (int i = 0; i < ret.Count; i++)
            {
                var dist = ret[_sampleCoordList[i]].Min();
                indDot++;
                if (dist < 100000)
                {
                    if (dist > 50000)
                    {
                        Console.WriteLine("Inacrurrate!");
                        _bmp.Save(FFInaccurateResultsFolderPath + "NUMBERREAD" + DateTime.UtcNow.Ticks.ToString() + ".bmp");
                    }
                    if (number != "" && indDot > (_numbersDimension.Width + 1) && !dotFound)
                    {
                        number += ",";
                        dotFound = true;
                    }
                    indDot = 0;
                    number += _referenceList[ret[_sampleCoordList[i]].IndexOf(dist)].Item1;

                }
            }

            if (number == "")
                return 0;
            return Decimal.Parse(number);
        }

        public void StartMonitoring()
        {
            if (FFIsMonitoring == false)
            {
                InitializeMonitoringMembers();
                FFIsMonitoring = true;
                FFMonitoringTask = Task.Run(() =>
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    while (FFIsMonitoring)
                    {
                        watch.Restart();
                        UpdateCurrentImage();
                        var currentImg = PCurrentBmp;

                        foreach (var item in FFMonitoringFunctionList)
                        {
                            item(currentImg);
                        }

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        //Console.WriteLine("Monitoring Time Taken: {0}ms", elapsedMs);
                        Thread.Sleep(PMonitoringWaitTimeMs);
                    }
                });
            }
        }

        public void StopMonitoring()
        {
            FFIsMonitoring = false;
        }

        protected abstract void InitializeMonitoringMembers();
    }
}
