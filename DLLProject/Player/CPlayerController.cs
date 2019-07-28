using ScreenScraping.Readers.LauncherReader.EspaceJeux;
using ScreenScraping.Readers.TableReader.TwoMax;
using ScreenScraping.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Player
{
    public class CPlayerController : IPlayer
    {
        protected CReaderController FFLauncherReader;
        protected List<CTableReaderTwoMax> FFTableReaderList;
        protected WindowsInputDLL.CInputControllerSilence FFInputController;
        protected Resources.Launcher.EspaceJeux.Constantes FFConstants;
        protected Resources.TwoMax.EspaceJeux.Constantes FFTableConstants;
        protected Dictionary<CReaderController.Filters_Chk_boxes, Rectangle> FFChkDic;
        protected Task FFCurrentTask = null;
        protected IntPtr FFHwnd;
        protected Random FFRandom;
        protected List<Tuple<Rectangle, Bitmap>> FFOpenedTableTableNameBitmapList;
        protected Semaphore FFInputSemaphore;
        const int screenWidthStride = 1920 / 3; // assumed 1920 x 1080 resolution
        const int screenHeightStride = 1080 / 2;

        public CPlayerController(IntPtr _launcherHandle)
        {
            FFLauncherReader = new CReaderController(_launcherHandle, ScreenCapture.PInstance);
            FFTableReaderList = new List<CTableReaderTwoMax>();
            FFInputController = new WindowsInputDLL.CInputControllerSilence();
            FFTableConstants = new Resources.TwoMax.EspaceJeux.Constantes();
            FFConstants = new Resources.Launcher.EspaceJeux.Constantes();
            FFRandom = new Random((int)DateTime.UtcNow.Ticks);
            FFHwnd = _launcherHandle;
            FFInputSemaphore = new Semaphore(0, 1);
            FFOpenedTableTableNameBitmapList = new List<Tuple<Rectangle, Bitmap>>();
            FFChkDic = new Dictionary<CReaderController.Filters_Chk_boxes, System.Drawing.Rectangle>();
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.TwoSeats, FFConstants.FILTERS_CHK_TWO_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.ThreeSeats, FFConstants.FILTERS_CHK_THREE_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.FourSeats, FFConstants.FILTERS_CHK_FOUR_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.FiveSeats, FFConstants.FILTERS_CHK_FIVE_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.SixSeats, FFConstants.FILTERS_CHK_SIX_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.SevenSeats, FFConstants.FILTERS_CHK_SEVEN_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.EightSeats, FFConstants.FILTERS_CHK_EIGHT_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.NineSeats, FFConstants.FILTERS_CHK_NINE_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.TenSeats, FFConstants.FILTERS_CHK_TEN_SEATS);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.Empty, FFConstants.FILTERS_CHK_EMPTY);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.Full, FFConstants.FILTERS_CHK_FULL);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.Regular, FFConstants.FILTERS_CHK_REGULAR);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.Turbo, FFConstants.FILTERS_CHK_TURBO);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.NoLimit, FFConstants.FILTERS_CHK_NO_LIMIT);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.PotLimit, FFConstants.FILTERS_CHK_POT_LIMIT);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.Limit, FFConstants.FILTERS_CHK_LIMIT);
            FFChkDic.Add(CReaderController.Filters_Chk_boxes.UseFilters, FFConstants.FILTERS_USE_FILTERS);
        }

        protected Point GetWindowLocation(IntPtr _hwnd)
        {
            var r = new WindowHelper.Rect();
            WindowHelper.GetWindowRect(_hwnd, ref r);
            return new Point(r.Left, r.Top);
        }

        #region Launcher

        protected Point GetLauncherWindowLocation()
        {
            return GetWindowLocation(FFHwnd);
        }

        protected void DisplayFiltersSettings()
        {

            var windowLocation = GetLauncherWindowLocation();
            FFLauncherReader.UpdateCurrentImage();
            var chkEnabled = FFLauncherReader.FiltersChkBoxesEnabled(FFLauncherReader.PCurrentBmp);
            while ((chkEnabled & CReaderController.Filters_Chk_boxes.UseFilters) == CReaderController.Filters_Chk_boxes.None)
            {
                var region = FFConstants.FILTERS_USE_FILTERS;
                region.X += windowLocation.X;
                region.Y += windowLocation.Y;
                FFInputController.MoveMouseFromCurrentLocation(region, true);
                Thread.Sleep(100);
                FFLauncherReader.UpdateCurrentImage();
                chkEnabled = FFLauncherReader.FiltersChkBoxesEnabled(FFLauncherReader.PCurrentBmp);
            }

            while (!FFLauncherReader.IsFiltersSettingDisplayed(FFLauncherReader.PCurrentBmp))
            {
                var region = FFConstants.FILTERS_BUTTON;
                region.X += windowLocation.X;
                region.Y += windowLocation.Y;
                FFInputController.MoveMouseFromCurrentLocation(region, true);
                Thread.Sleep(100);
                FFLauncherReader.UpdateCurrentImage();
            }
        }

        protected void SetFiltersChkBoxes(
            CReaderController.Filters_Chk_boxes _flags =
                    CReaderController.Filters_Chk_boxes.TwoSeats |
                    CReaderController.Filters_Chk_boxes.Full |
                    CReaderController.Filters_Chk_boxes.Regular |
                    CReaderController.Filters_Chk_boxes.NoLimit |
                    CReaderController.Filters_Chk_boxes.UseFilters
            )
        {

            DisplayFiltersSettings();
            var windowLocation = GetLauncherWindowLocation();
            var chkBoxEnabled = FFLauncherReader.FiltersChkBoxesEnabled();

            for (int i = 1; i < (int)CReaderController.Filters_Chk_boxes.UseFilters << 1; i *= 2)
            {
                var opt = (CReaderController.Filters_Chk_boxes)i;
                if (((chkBoxEnabled ^ _flags) & opt) != CReaderController.Filters_Chk_boxes.None)
                {
                    var r = FFChkDic[opt];
                    r.X += windowLocation.X + 2;
                    r.Y += windowLocation.Y + 2;
                    FFInputController.MoveMouseFromCurrentLocation(r, true);
                }
            }

            chkBoxEnabled = FFLauncherReader.FiltersChkBoxesEnabled();
            if (chkBoxEnabled != _flags)
            {
                SetFiltersChkBoxes(_flags);
            }

        }

        protected void SetFiltersValues(Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values _val, CReaderController.Filters_Drop_Down_Option _opt, Rectangle _drpDownButton, Rectangle _downArrow, Rectangle _upArrow)
        {

            DisplayFiltersSettings();
            var windowLocation = GetLauncherWindowLocation();
            windowLocation.X += 0;
            windowLocation.Y += 0;
            var displayedValues = FFLauncherReader.FiltersDropDownValuesDisplayed(_opt);
            bool goUp = true;
            bool hasChanged = true;

            while (displayedValues.First().Item1 != _val)
            {
                while (displayedValues.Count != 4)
                {
                    var r = windowLocation;
                    r.X += _drpDownButton.X;
                    r.Y += _drpDownButton.Y;
                    FFInputController.MoveMouseFromCurrentLocation(new System.Drawing.Rectangle(r.X, r.Y, _drpDownButton.Width, _drpDownButton.Height), true);
                    hasChanged = true;
                    Thread.Sleep(100);
                    displayedValues = FFLauncherReader.FiltersDropDownValuesDisplayed(_opt);

                }
                if (displayedValues[1].Item1 == Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values.None || displayedValues[3].Item1 == Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values.NL5000)
                {
                    goUp = !goUp;
                    hasChanged = true;
                }

                Rectangle region;
                if (goUp)
                {
                    region = _upArrow;
                }
                else
                {
                    region = _downArrow;
                }

                region.X += windowLocation.X;
                region.Y += windowLocation.Y;
                if (hasChanged)
                {
                    FFInputController.MoveMouseFromCurrentLocation(region);
                    hasChanged = false;
                }


                for (int i = 0; i < FFRandom.Next(1, 4); i++)
                {
                    FFInputController.MouseClickFromCurrentLocation();
                }

                Thread.Sleep(FFRandom.Next(50, 250));
                displayedValues = FFLauncherReader.FiltersDropDownValuesDisplayed(_opt);
                if (displayedValues.Any(x => x.Item1 == _val))
                {
                    var coord = displayedValues.Where(x => x.Item1 == _val).First().Item2;
                    FFInputController.MoveMouseFromCurrentLocation(new System.Drawing.Rectangle(coord.X + windowLocation.X, coord.Y + windowLocation.Y, _drpDownButton.Width, _drpDownButton.Height), true);
                    Thread.Sleep(FFRandom.Next(50, 200));
                    displayedValues = FFLauncherReader.FiltersDropDownValuesDisplayed(_opt);
                    hasChanged = true;
                }
            }

        }

        protected void SetFiltersValues(
            Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values _min =
                    Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values.None,
            Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values _max =
                    Resources.Launcher.EspaceJeux.References.Filters_Drop_Down_Values.NL5000)
        {

            var drpDownButton = FFConstants.FILTERS_MIN_DROP_DOWN_BUTTON;
            var downArrow = FFConstants.FILTERS_MIN_DROP_DOWN_DOWN_ARROW;
            var upArrow = FFConstants.FILTERS_MIN_DROP_DOWN_UP_ARROW;
            SetFiltersValues(_min, CReaderController.Filters_Drop_Down_Option.Min, drpDownButton, downArrow, upArrow);
            drpDownButton = FFConstants.FILTERS_MAX_DROP_DOWN_BUTTON;
            downArrow = FFConstants.FILTERS_MAX_DROP_DOWN_DOWN_ARROW;
            upArrow = FFConstants.FILTERS_MAX_DROP_DOWN_UP_ARROW;
            SetFiltersValues(_max, CReaderController.Filters_Drop_Down_Option.Max, drpDownButton, downArrow, upArrow);

        }

        protected void SetTableGridPlrsSorting()
        {

            var windowLocation = GetLauncherWindowLocation();
            while (!FFLauncherReader.IsTablePlrsOrdered())
            {
                var region = FFConstants.TABLEGRID_PLRS_SORTING_DECREMENTING;
                region.X += windowLocation.X;
                region.Y += windowLocation.Y;
                FFInputController.MoveMouseFromCurrentLocation(region);
                FFInputController.MouseClickFromCurrentLocation();
                Thread.Sleep(100);
            }

        }

        protected void OpenFullTables()
        {

            var windowLocation = GetLauncherWindowLocation();
            var tables = FFLauncherReader.GetRowOfFullTables();
            foreach (var tableRec in tables)
            {
                var rec = tableRec;
                rec.X += windowLocation.X;
                rec.Y += windowLocation.Y;
                FFInputController.MoveMouseFromCurrentLocation(rec);
                FFInputController.MouseClickFromCurrentLocation();
                FFInputController.MouseClickFromCurrentLocation();
            }

        }

        public void SetFilters()
        {

            SetFiltersChkBoxes();
            SetFiltersValues();
            SetTableGridPlrsSorting();
        }

        public void MonitorFullTablesAndPopup()
        {
            FFLauncherReader.ELauncherFullTable += OnNewFullTable;
            FFLauncherReader.EPopup += OnLauncherPopup;
            FFLauncherReader.StartMonitoring();
        }

        #endregion

        #region Table

        public void AddNewTables()
        {
            var processeList = WindowHelper.GetProcessListFromMainWindowTitle("EspaceJeux");
            var tableList = processeList.Where(x => x.MainWindowTitle.Contains("Table"));
            foreach (var table in tableList)
            {
                if (!FFTableReaderList.Any(x => x.PHwnd == table.MainWindowHandle))
                {
                    FFTableReaderList.Add(new ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController(table.MainWindowHandle, ScreenScraping.ScreenCapture.ScreenCapture.PInstance));
                    var newTable = FFTableReaderList.Last();
                    newTable.EPlayerCountChange += OnPlayerCountChange;
                    newTable.EOurTurn += OnOurTurn;
                    newTable.PMonitoringWaitTimeMs = 1500;
                    newTable.StartMonitoring();
                }
            }
        }

        public void PositionTables()
        {

            for (int i = 0; i < FFTableReaderList.Count; i++)
            {
                var t = FFTableReaderList[i];
                WindowHelper.SetWindowRegion(t.PHwnd, FFTableConstants.WINDOW_DIMENSION.Width, FFTableConstants.WINDOW_DIMENSION.Height, (i % 3) * screenWidthStride, (i / 3) * screenHeightStride, WindowHelper.WindowsFlags.SWP_SHOWWINDOW | WindowHelper.WindowsFlags.SWP_NOZORDER);
            }
        }

        public void CloseTable(ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController _tableReader)
        {
            _tableReader.StopMonitoring();
            FFTableReaderList.Remove(_tableReader);
            WindowHelper.SetWindowRegion(_tableReader.PHwnd, 0, 0, 0, 0, WindowHelper.WindowsFlags.SWP_SHOWWINDOW | WindowHelper.WindowsFlags.SWP_NOSIZE | WindowHelper.WindowsFlags.SWP_NOZORDER | WindowHelper.WindowsFlags.SWP_NOMOVE);
            var windowLocation = GetWindowLocation(_tableReader.PHwnd);
            var region = FFTableConstants.WINDOW_BUTTON_X;
            region.X += windowLocation.X;
            region.Y += windowLocation.Y;
            FFInputController.MoveMouseFromCurrentLocation(region, true);
        }

        public void CloseAllTables()
        {
            while (FFTableReaderList.Count > 0)
            {
                CloseTable((ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController)FFTableReaderList.First());
            }
        }

        #region PlayingActions

        protected void Check(IntPtr _windowHandle)
        {
            Point windowLocation = GetWindowLocation(_windowHandle);
            Rectangle checkButton = FFTableConstants.ACTION_BUTTON_CHECK;
            checkButton.Location = new Point(checkButton.X + windowLocation.X, checkButton.Y + windowLocation.Y);

            FFInputController.MoveMouseFromCurrentLocation(checkButton, true);
        }

        protected void Bet(IntPtr _windowHandle, decimal _value)
        {
            Point windowLocation = GetWindowLocation(_windowHandle);
            Rectangle betTextBox = FFTableConstants.ACTION_TXT_BET;
            betTextBox.Location = new Point(betTextBox.X + windowLocation.X, betTextBox.Y + windowLocation.Y);
            Rectangle betButton = FFTableConstants.ACTION_BUTTON_BET;
            betButton.Location = new Point(betButton.X + windowLocation.X, betButton.Y + windowLocation.Y);

            FFInputController.MoveMouseFromCurrentLocation(betTextBox, true);
            FFInputController.InputString(_value.ToString());
            FFInputController.MoveMouseFromCurrentLocation(betButton, true);
        }

        protected void Call(IntPtr _windowHandle)
        {
            Point windowLocation = GetWindowLocation(_windowHandle);
            Rectangle callButton = FFTableConstants.ACTION_BUTTON_CALL;
            callButton.Location = new Point(callButton.X + windowLocation.X, callButton.Y + windowLocation.Y);

            FFInputController.MoveMouseFromCurrentLocation(callButton, true);
        }

        protected void Raise(IntPtr _windowHandle, decimal _value)
        {
            Point windowLocation = GetWindowLocation(_windowHandle);
            Rectangle raiseTextBox = FFTableConstants.ACTION_TXT_RAISE;
            raiseTextBox.Location = new Point(raiseTextBox.X + windowLocation.X, raiseTextBox.Y + windowLocation.Y);
            Rectangle raiseButton = FFTableConstants.ACTION_BUTTON_RAISE;
            raiseButton.Location = new Point(raiseButton.X + windowLocation.X, raiseButton.Y + windowLocation.Y);

            FFInputController.MoveMouseFromCurrentLocation(raiseTextBox, true);
            FFInputController.InputString(_value.ToString());
            FFInputController.MoveMouseFromCurrentLocation(raiseButton, true);
        }

        protected void Fold(IntPtr _windowHandle)
        {
            Point windowLocation = GetWindowLocation(_windowHandle);
            Rectangle foldButton = FFTableConstants.ACTION_BUTTON_FOLD;
            foldButton.Location = new Point(foldButton.X + windowLocation.X, foldButton.Y + windowLocation.Y);

            FFInputController.MoveMouseFromCurrentLocation(foldButton, true);
        }

        #endregion

        #endregion

        #region Events

        #region LauncherEvents

        public void OnNewFullTable(ScreenScraping.Readers.LauncherReader.EspaceJeux.CReaderController sender, ScreenScraping.Readers.Events.LauncherFullTableEventArgs arg)
        {
            Console.WriteLine("OnNewFullTableEvent");
            lock (FFInputSemaphore)
            {
                if (FFTableReaderList.Count < 6)
                {
                    //FFOpenedTableTableNameBitmapList.Add(new Tuple<Rectangle, Bitmap>(arg.PRegion, arg.PTableName));
                    var windowLocation = GetLauncherWindowLocation();
                    arg.PRegion.X += windowLocation.X;
                    arg.PRegion.Y += windowLocation.Y;
                    FFInputController.MoveMouseFromCurrentLocation(arg.PRegion);
                    FFInputController.MouseClickFromCurrentLocation();
                    FFInputController.MouseClickFromCurrentLocation();
                    Thread.Sleep(3000);
                    AddNewTables();
                    PositionTables();
                }
            }
        }

        public void OnLauncherPopup(ScreenScraping.Readers.LauncherReader.EspaceJeux.CReaderController sender, ScreenScraping.Readers.Events.PopupEventArgs arg)
        {
            lock (FFInputSemaphore)
            {
                Console.WriteLine("OnLauncherPopup");
                var windowLocation = GetWindowLocation(sender.PHwnd);
                if (arg.PEventType == ScreenScraping.Readers.Events.EventType.PlayedForHoursEvent)
                {

                    var region = new Rectangle(new Point(windowLocation.X + FFConstants.POPUP_YES.X, windowLocation.Y + FFConstants.POPUP_YES.Y), FFConstants.POPUP_YES.Size);

                    FFInputController.MoveMouseFromCurrentLocation(region, true);
                }
                else if (arg.PEventType == ScreenScraping.Readers.Events.EventType.TournamentAnnouncementEvent)
                {
                    var region = new Rectangle(new Point(windowLocation.X + FFConstants.POPUP_CANCEL.X, windowLocation.Y + FFConstants.POPUP_CANCEL.Y), FFConstants.POPUP_CANCEL.Size);

                    FFInputController.MoveMouseFromCurrentLocation(region, true);
                }
            }
        }

        #endregion

        #region TableEvents

        public void OnPlayerCountChange(ScreenScraping.Readers.TableReader.TwoMax.CTableReaderTwoMax sender, ScreenScraping.Readers.Events.PlayerCountEventArgs arg)
        {
            lock (FFInputSemaphore)
            {
                Console.WriteLine("OnNewFullTableEvent");
                //FFInputSemaphore.WaitOne();
                if (arg.PPlayerCount != 2)
                {
                    CloseTable((ScreenScraping.Readers.TableReader.TwoMax.EspaceJeux.CReaderController)FFTableReaderList.Where(x => x.PHwnd == arg.PHwnd).First());
                }
                //FFInputSemaphore.Release();
            }
        }

        public void OnOurTurn(CTableReaderTwoMax sender, ScreenScraping.Readers.Events.OurTurnEventArgs arg)
        {
            lock (FFInputSemaphore)
            {
                Console.WriteLine("OnOurTurnEvent");
                arg.PBmp.Save(@"C:\Users\admin\Desktop\ReferenceBMP\MonitoringSampleV2\" + DateTime.UtcNow.Ticks.ToString() + ".bmp");

            }
        }

        #endregion

        #endregion
    }
}
