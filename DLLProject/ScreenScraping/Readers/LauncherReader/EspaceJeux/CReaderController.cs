using ScreenScraping.Readers.Events;
using ScreenScraping.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.LauncherReader.EspaceJeux
{
    public class CReaderController : CAbstractReader, ILauncherReader
    {
        private Resources.Launcher.EspaceJeux.Constantes FFConstants;
        private Resources.Launcher.EspaceJeux.References FFReferences;
        public CReaderController(
            IntPtr _hwnd,
            IScreenShotHelper _screenShotHelper) : base(_hwnd, _screenShotHelper)
        {
            FFConstants = new Resources.Launcher.EspaceJeux.Constantes();
            FFReferences = new Resources.Launcher.EspaceJeux.References();

            FFMonitoringFunctionList.Add(MonitorFullTables);
            FFMonitoringFunctionList.Add(MonitorPopup);
        }

        [Flags]
        public enum Filters_Chk_boxes
        {
            None        = 0,
            TwoSeats    = 0x1 << 0,
            ThreeSeats  = 0x1 << 1,
            FourSeats   = 0x1 << 2,
            FiveSeats   = 0x1 << 3,
            SixSeats    = 0x1 << 4,
            SevenSeats  = 0x1 << 5,
            EightSeats  = 0x1 << 6,
            NineSeats   = 0x1 << 7,
            TenSeats    = 0x1 << 8,
            Empty       = 0x1 << 9,
            Full        = 0x1 << 10,
            Regular     = 0x1 << 11,
            Turbo       = 0x1 << 12,
            NoLimit     = 0x1 << 13,
            PotLimit    = 0x1 << 14,
            Limit       = 0x1 << 15,
            UseFilters  = 0x1 << 16,
            All         = -1
        }

        public enum Filters_Drop_Down_Option
        {
            Min,
            Max
        }

        public bool IsFiltersSettingDisplayed(Bitmap _bmp)
        {
            var referenceList = new List<Bitmap>();

            referenceList.Add(FFReferences.PFiltersChkEnabledBmp);
            referenceList.Add(FFReferences.PFiltersChkDisabledBmp);

            var res = OpenCL.OpenCLController.CalculateDistances(_bmp, FFConstants.FILTERS_CHK_EIGHT_SEATS.Location, referenceList);

            return res.Min() < 10000;
        }

        public bool IsFiltersSettingDisplayed()
        {
            UpdateCurrentImage();
            return IsFiltersSettingDisplayed(PCurrentBmp);
        }

        public Filters_Chk_boxes FiltersChkBoxesEnabled(Bitmap _bmp)
        {

            var referenceList = new List<Bitmap>();
            referenceList.Add(FFReferences.PFiltersChkEnabledBmp);

            var sampleCoordList = new List<Point>();
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_TWO_SEATS.X,
                FFConstants.FILTERS_CHK_TWO_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_THREE_SEATS.X,
                FFConstants.FILTERS_CHK_THREE_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_FOUR_SEATS.X,
                FFConstants.FILTERS_CHK_FOUR_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_FIVE_SEATS.X,
                FFConstants.FILTERS_CHK_FIVE_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_SIX_SEATS.X,
                FFConstants.FILTERS_CHK_SIX_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_SEVEN_SEATS.X,
                FFConstants.FILTERS_CHK_SEVEN_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_EIGHT_SEATS.X,
                FFConstants.FILTERS_CHK_EIGHT_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_NINE_SEATS.X,
                FFConstants.FILTERS_CHK_NINE_SEATS.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_TEN_SEATS.X,
                FFConstants.FILTERS_CHK_TEN_SEATS.Y));

            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_EMPTY.X,
                FFConstants.FILTERS_CHK_EMPTY.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_FULL.X,
                FFConstants.FILTERS_CHK_FULL.Y));

            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_REGULAR.X,
                FFConstants.FILTERS_CHK_REGULAR.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_TURBO.X,
                FFConstants.FILTERS_CHK_TURBO.Y));

            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_NO_LIMIT.X,
                FFConstants.FILTERS_CHK_NO_LIMIT.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_POT_LIMIT.X,
                FFConstants.FILTERS_CHK_POT_LIMIT.Y));
            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_CHK_LIMIT.X,
                FFConstants.FILTERS_CHK_LIMIT.Y));

            sampleCoordList.Add(new Point(
                FFConstants.FILTERS_USE_FILTERS.X,
                FFConstants.FILTERS_USE_FILTERS.Y));


            var res = OpenCL.OpenCLController.CalculateDistances(_bmp, sampleCoordList, referenceList);

            Filters_Chk_boxes ret = 0;

            for (int i = 0; i < res.Count; i++)
            {
                var dist = res[sampleCoordList[i]].Min();
                if (dist < 10000)
                {
                    ret |= (Filters_Chk_boxes)(0x1 << i);
                }
            }
            return ret;
        }

        public Filters_Chk_boxes FiltersChkBoxesEnabled()
        {
            UpdateCurrentImage();
            return FiltersChkBoxesEnabled(PCurrentBmp);
        }


            public List<Tuple<Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values, Point>> FiltersDropDownValuesDisplayed(Filters_Drop_Down_Option _val)
        {
            UpdateCurrentImage();
            var sample = PCurrentBmp;

            var referenceList = new List<Bitmap>();
            referenceList = FFReferences.PFiltersDropDownValues.Select(x => x.Value).ToList();

            var sampleCoordList = new List<Point>();
            if (_val == Filters_Drop_Down_Option.Min)
            {
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MIN_VALUE.X,
                    FFConstants.FILTERS_MIN_VALUE.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_1.X,
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_1.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_2.X,
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_2.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_3.X,
                    FFConstants.FILTERS_MIN_DROP_DOWN_OPTION_3.Y));
            }
            else
            {
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MAX_VALUE.X,
                    FFConstants.FILTERS_MAX_VALUE.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_1.X,
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_1.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_2.X,
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_2.Y));
                sampleCoordList.Add(new Point(
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_3.X,
                    FFConstants.FILTERS_MAX_DROP_DOWN_OPTION_3.Y));
            }


            var res = OpenCL.OpenCLController.CalculateDistances(sample, sampleCoordList, referenceList);

            var ret = new List<Tuple<Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values, Point>>();

            for (int i = 0; i < res.Count; i++)
            {
                var min = res[sampleCoordList[i]].Min();
                if (min > 10000)
                {
                    continue;
                }
                var indMin = res[sampleCoordList[i]].IndexOf(min);
                var val = FFReferences.PFiltersDropDownValues.Where(x => x.Value == referenceList[indMin]).First().Key;
                ret.Add(new Tuple<Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values, Point>(val, sampleCoordList[i]));
            }
            return ret;
        }

        public bool IsTablePlrsOrdered()
        {
            UpdateCurrentImage();
            var sample = PCurrentBmp;

            var ret = OpenCL.OpenCLController.CalculateDistances(sample, FFConstants.TABLEGRID_PLRS_SORTING_DECREMENTING.Location, FFReferences.PTableGridPlrsSortingDecrementing);
            return ret < 10000;
        }

        public List<Rectangle> GetRowOfFullTables(Bitmap _bmp)
        {
            var referenceList = new List<Bitmap>();
            referenceList.Add(FFReferences.PTableGridPlrs_2_2.Item1);
            referenceList.Add(FFReferences.PTableGridPlrs_2_2.Item2);
            referenceList.Add(FFReferences.PTableGridPlrs_2_2.Item3);

            var nbRow = FFConstants.TABLEGRID_ROW_DIM.Y;
            var sampleCoordList = new List<Point>();
            for (int i = 0; i < nbRow; i++)
            {
                sampleCoordList.Add(new Point(
                    FFConstants.TABLEGRID_TABLE_1_PLRS.X,
                    FFConstants.TABLEGRID_TABLE_1_PLRS.Y + i * FFConstants.TABLEGRID_ROW_DIM.Height));
            }

            var ret = OpenCL.OpenCLController.CalculateDistances(_bmp, sampleCoordList, referenceList);
            var retVal = new List<Rectangle>();

            for (int i = 0; i < ret.Count; i++)
            {
                var min = ret[sampleCoordList[i]].Min();
                if (min < 10000)
                {
                    var region = FFConstants.TABLEGRID_ROW_1;
                    region.Y += FFConstants.TABLEGRID_ROW_DIM.Height * i;
                    retVal.Add(region);
                }
            }

            return retVal;
        }
        public List<Rectangle> GetRowOfFullTables()
        {
            UpdateCurrentImage();
            return GetRowOfFullTables(PCurrentBmp);
        }


        #region Events

        #region FullTableRowDetectedEvent

        private List<decimal> FFMMFullTableBitmapList;

        public delegate void LauncherFullTableEventHandler(CReaderController sender, LauncherFullTableEventArgs e);
        public event LauncherFullTableEventHandler ELauncherFullTable;
        protected void MonitorFullTables(Bitmap _bmp)
        {
            if (ELauncherFullTable != null)
            {
                var rowList = GetRowOfFullTables(_bmp);

                var rowIdList = new List<Tuple<Rectangle, decimal>>();

                foreach (var row in rowList)
                {
                    var region = FFConstants.TABLE_GRID_TABLE_1_TABLENAME_NUMBERS;
                    region.Y = row.Y;
                    var sampleCoordList = new List<Point>();
                    for (int i = 0; i < FFConstants.TABLE_GRID_TABLE_1_TABLENAME_NUMBERS.Width - FFReferences.PTableGridCharacterList.First().Item2.Width; i++)
                    {
                        var tempRegion = region.Location;
                        tempRegion.X += i;
                        sampleCoordList.Add(tempRegion);
                    }
                    var id = GetNumbers(_bmp, sampleCoordList, FFReferences.PTableGridCharacterList, new Rectangle(new Point(0,0), FFReferences.PTableGridCharacterList.First().Item2.Size));
                    rowIdList.Add(new Tuple<Rectangle, decimal>(region, id));
                }

                FFMMFullTableBitmapList.RemoveAll(x => !rowIdList.Any(y => y.Item2 == x));

                foreach (var id in rowIdList)
                {
                    if (!FFMMFullTableBitmapList.Contains(id.Item2))
                    {
                        var e = new LauncherFullTableEventArgs();
                        e.PBNewImg = true;
                        e.PRegion = id.Item1;
                        e.PTableName = _bmp.Clone(id.Item1, _bmp.PixelFormat);
                        FFMMFullTableBitmapList.Add(id.Item2);
                        ELauncherFullTable?.Invoke(this, e);
                    }
                }
            }
        }

        #endregion

        #region PopUpEvent

        public delegate void LauncherPopupEventHandler(CReaderController sender, PopupEventArgs e);
        public event LauncherPopupEventHandler EPopup;
        protected void MonitorPopup(Bitmap _bmp)
        {
            if (EPopup != null)
            {
                var referenceList = new List<Bitmap>();
                referenceList.Add(FFReferences.PPopupQuestionBmp);
                referenceList.Add(FFReferences.PPopupExclamationBmp.Item1);
                referenceList.Add(FFReferences.PPopupExclamationBmp.Item2);


                var res = OpenCL.OpenCLController.CalculateDistances(_bmp, FFConstants.POPUP_CIRCLE.Location, referenceList);

                var eventArgs = new PopupEventArgs();

                if (res[0] < 10000)
                {
                    eventArgs.PEventType = EventType.PlayedForHoursEvent;
                    EPopup?.Invoke(this, eventArgs);
                }
                else if (res[1] < 10000 || res[2] < 10000)
                {
                    eventArgs.PEventType = EventType.TournamentAnnouncementEvent;
                    EPopup?.Invoke(this, eventArgs);
                }
            }
        }

        #endregion

        protected override void InitializeMonitoringMembers()
        {
            FFMMFullTableBitmapList = new List<decimal>();
        }
        #endregion



    }
}
