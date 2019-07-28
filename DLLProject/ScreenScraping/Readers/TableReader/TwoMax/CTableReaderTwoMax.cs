using PokerShared;
using Resources;
using ScreenScraping.Readers.Events;
using ScreenScraping.ScreenCapture;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenScraping.Readers.TableReader.TwoMax
{
    public abstract class CTableReaderTwoMax : CTableReader
    {
        [Flags]
        public enum PlayerPosition
        {
            None = 0,
            P0 = 0x1 << 0,
            P1 = 0x1 << 1
        }

        #region Members

        protected const int FFAccuracyThreshold = 500 * 1000;

        #endregion

        #region Constructor

        protected CTableReaderTwoMax(IntPtr _hwnd, CReferences _references, CConstantsTable _constants, IScreenShotHelper _screenShotHelper) : base(_hwnd, _references, _constants, _screenShotHelper)
        {
            FFMonitoringFunctionList.Add(MonitorOurTurn);
            FFMonitoringFunctionList.Add(MonitorPlayerCount);
            FFMonitoringFunctionList.Add(MonitorRiverCard);
        }

        #endregion

        #region Miscellaneous

        #region IsOurTurn
        public virtual bool IsOurTurn(Bitmap _bmp, ref int _distance)
        {
            _distance = OpenCL.OpenCLController.CalculateDistances(_bmp, FFConstants.PLAYER_1_TURN.Location, FFReferences.PLAYER_1_TURN_INDICATOR_BMP);

            return _distance < FFAccuracyThreshold;
        }

        public virtual bool IsOurTurn(Bitmap _bmp)
        {
            int temp = 0;
            return IsOurTurn(_bmp, ref temp);
        }

        public override bool IsOurTurn()
        {
            UpdateCurrentImage();
            return IsOurTurn(PCurrentBmp);
        }
        #endregion

        #region IsHeroDealer

        public virtual bool IsHeroDealer(Bitmap _bmp, ref int _distance)
        {
            _distance = OpenCL.OpenCLController.CalculateDistances(_bmp, FFConstants.PLAYER_1_DEALER.Location, FFReferences.PDealerButtonBmp);

            return _distance < FFAccuracyThreshold;
        }

        public virtual bool IsHeroDealer(Bitmap _bmp)
        {
            int temp = 0;
            return IsHeroDealer(_bmp, ref temp);
        }

        public override bool IsHeroDealer()
        {
            UpdateCurrentImage();
            return IsHeroDealer(PCurrentBmp);
        }

        #endregion

        #region IsPlayerPresent

        public virtual PlayerPosition GetSeatedPlayers(Bitmap _bmp)
        {
            var sampleCoordList = new List<Point>();
            var referenceList = new List<Bitmap>();

            sampleCoordList.Add(new Point(
                FFConstants.PLAYER_0_EMPTY.X,
                FFConstants.PLAYER_0_EMPTY.Y));
            sampleCoordList.Add(new Point(
                FFConstants.PLAYER_1_EMPTY.X,
                FFConstants.PLAYER_1_EMPTY.Y));
            referenceList.Add(FFReferences.PPlayerEmptyBmpTuple.Item1);
            referenceList.Add(FFReferences.PPlayerEmptyBmpTuple.Item2);


            var res = OpenCL.OpenCLController.CalculateDistances(_bmp, sampleCoordList, referenceList);

            var retVal = PlayerPosition.None;

            for (int i = 0; i < res.Count; i++)
            {
                if (res[sampleCoordList[i]].Min() > 10000)
                {
                    retVal |= (PlayerPosition)(0x1 << i);
                }
            }

            return retVal;
        }

        public virtual PlayerPosition GetSeatedPlayers()
        {
            UpdateCurrentImage();
            return GetSeatedPlayers(PCurrentBmp);
        }

        #endregion

        #endregion

        #region Cards
        //List<Tuple<CCard, typeDistance, valueDistance>
        //Return a list of Card with the typeDistance and valueDistance in this order of the tuple
        protected virtual List<Tuple<CCard, int, int>> GetCardList(Bitmap _bmp, List<Point> _sampleTypeCoordList, List<Point> _sampleValueCoordList, List<Tuple<CCard.Type, Bitmap>> _typeReferenceList, List<Tuple<CCard.Value, Bitmap>> _valueReferenceList)
        {
            var typeReferenceList = _typeReferenceList.ConvertAll(x => x.Item2);
            var valueReferenceList = _valueReferenceList.ConvertAll(x => x.Item2);


            var typeResultList =
                OpenCL.OpenCLController.CalculateDistances(_bmp, _sampleTypeCoordList, typeReferenceList);
            var valueResultList =
                OpenCL.OpenCLController.CalculateDistances(_bmp, _sampleValueCoordList, valueReferenceList);

            var resultList = new List<Tuple<CCard, int, int>>();

            for (int i = 0; i < typeResultList.Count; i++)
            {
                var indCoord = i % _sampleTypeCoordList.Count();

                var typeDistance = typeResultList[_sampleTypeCoordList[indCoord]].Min();
                var t = _typeReferenceList.Find(x =>
                    x.Item2 == typeReferenceList[typeResultList[_sampleTypeCoordList[indCoord]].IndexOf(typeDistance)]).Item1;
                var valueDistance = valueResultList[_sampleValueCoordList[indCoord]].Min();
                var v = _valueReferenceList.Find(x =>
                    x.Item2 == valueReferenceList[valueResultList[_sampleValueCoordList[indCoord]].IndexOf(valueDistance)]).Item1;

                resultList.Add(new Tuple<CCard, int, int>(new CCard(v, t), typeDistance, valueDistance));
                if (typeDistance > FFAccuracyThreshold || valueDistance > FFAccuracyThreshold)
                {
                    var c = new CCard(v, t).ToString();
                    Console.WriteLine("Inaccurate Card Read (t: {0}, v: {1}); Saving Sample as reference; Card: {2}", typeDistance, valueDistance, c);
                    _bmp.Save(FFInaccurateResultsFolderPath + "InaccurateCardRead_" + c + "_(t " + typeDistance.ToString() + ", v " + valueDistance.ToString() + ")_" + DateTime.UtcNow.Ticks.ToString() + ".bmp");
                }
            }

            //sampleBmp.Dispose();

            return resultList;
        }

        #region Hand
        public virtual Tuple<CCard, CCard> GetHand(Bitmap _bmp, ref Tuple<Tuple<int, int>, Tuple<int, int>> _cardDistanceTuple)
        {
            var sampletypeCoordList = new List<Point>();
            sampletypeCoordList.Add(new Point(
                FFConstants.HAND_CARD1_TYPE.X,
                FFConstants.HAND_CARD1_TYPE.Y));
            sampletypeCoordList.Add(new Point(
                FFConstants.HAND_CARD2_TYPE.X,
                FFConstants.HAND_CARD2_TYPE.Y));
            var sampleValueCoordList = new List<Point>();
            sampleValueCoordList.Add(new Point(
                FFConstants.HAND_CARD1_VALUE.X,
                FFConstants.HAND_CARD1_VALUE.Y));
            sampleValueCoordList.Add(new Point(
                FFConstants.HAND_CARD2_VALUE.X,
                FFConstants.HAND_CARD2_VALUE.Y));

            var resultList = GetCardList(_bmp, sampletypeCoordList, sampleValueCoordList, FFReferences.PBoardCardTypeReferenceList, FFReferences.PHandCardValueReferenceList);

            _cardDistanceTuple = new Tuple<Tuple<int, int>, Tuple<int, int>>(new Tuple<int, int>(resultList[0].Item2, resultList[0].Item3), new Tuple<int, int>(resultList[1].Item2, resultList[1].Item3));
            return new Tuple<CCard, CCard>(resultList[0].Item1, resultList[1].Item1);
        }

        public virtual Tuple<CCard, CCard> GetHand(Bitmap _bmp)
        {
            var temp = new Tuple<Tuple<int, int>, Tuple<int, int>>(null, null);
            return GetHand(_bmp, ref temp);
        }

        public override Tuple<CCard, CCard> GetHand()
        {
            UpdateCurrentImage();
            return GetHand(PCurrentBmp);
        }
        #endregion

        #region Flop
        public virtual List<CCard> GetFlop(Bitmap _bmp, ref List<Tuple<int, int>> _cardDistanceList)
        {
            var sampletypeCoordList = new List<Point>();
            sampletypeCoordList.Add(new Point(
                FFConstants.BOARD_CARD1_TYPE.X,
                FFConstants.BOARD_CARD1_TYPE.Y));
            sampletypeCoordList.Add(new Point(
                FFConstants.BOARD_CARD2_TYPE.X,
                FFConstants.BOARD_CARD2_TYPE.Y));
            sampletypeCoordList.Add(new Point(
                FFConstants.BOARD_CARD3_TYPE.X,
                FFConstants.BOARD_CARD3_TYPE.Y));
            var sampleValueCoordList = new List<Point>();
            sampleValueCoordList.Add(new Point(
                FFConstants.BOARD_CARD1_VALUE.X,
                FFConstants.BOARD_CARD1_VALUE.Y));
            sampleValueCoordList.Add(new Point(
                FFConstants.BOARD_CARD2_VALUE.X,
                FFConstants.BOARD_CARD2_VALUE.Y));
            sampleValueCoordList.Add(new Point(
                FFConstants.BOARD_CARD3_VALUE.X,
                FFConstants.BOARD_CARD3_VALUE.Y));


            var resultList = GetCardList(_bmp, sampletypeCoordList, sampleValueCoordList, FFReferences.PBoardCardTypeReferenceList, FFReferences.PBoardCardValueReferenceList);

            var ret = resultList.ConvertAll(x => x.Item1).ToList();

            _cardDistanceList = resultList.ConvertAll(x => new Tuple<int, int>(x.Item2, x.Item3));

            return ret;
        }

        public virtual List<CCard> GetFlop(Bitmap _bmp)
        {
            var temp = new List<Tuple<int, int>>();
            return GetFlop(_bmp, ref temp);
        }

        public override List<CCard> GetFlop()
        {
            UpdateCurrentImage();
            return GetFlop(PCurrentBmp);
        }
        #endregion

        #region Turn
        public virtual CCard GetTurn(Bitmap _bmp, ref Tuple<int, int> _cardDistance)
        {
            var sampletypeCoordList = new List<Point>();
            sampletypeCoordList.Add(new Point(
                FFConstants.BOARD_CARD4_TYPE.X,
                FFConstants.BOARD_CARD4_TYPE.Y));
            var sampleValueCoordList = new List<Point>();
            sampleValueCoordList.Add(new Point(
                FFConstants.BOARD_CARD4_VALUE.X,
                FFConstants.BOARD_CARD4_VALUE.Y));

            var resultList = GetCardList(_bmp, sampletypeCoordList, sampleValueCoordList, FFReferences.PBoardCardTypeReferenceList, FFReferences.PBoardCardValueReferenceList);

            _cardDistance = new Tuple<int, int>(resultList[0].Item2, resultList[0].Item3);

            return resultList[0].Item1;
        }

        public virtual CCard GetTurn(Bitmap _bmp)
        {
            var temp = new Tuple<int, int>(0, 0);
            return GetTurn(_bmp, ref temp);
        }

        public override CCard GetTurn()
        {
            UpdateCurrentImage();
            return GetTurn(PCurrentBmp);
        }
        #endregion

        #region River
        public virtual CCard GetRiver(Bitmap _bmp, ref Tuple<int, int> _cardDistance)
        {
            var sampletypeCoordList = new List<Point>();
            sampletypeCoordList.Add(new Point(
                FFConstants.BOARD_CARD5_TYPE.X,
                FFConstants.BOARD_CARD5_TYPE.Y));
            var sampleValueCoordList = new List<Point>();
            sampleValueCoordList.Add(new Point(
                FFConstants.BOARD_CARD5_VALUE.X,
                FFConstants.BOARD_CARD5_VALUE.Y));

            var resultList = GetCardList(_bmp, sampletypeCoordList, sampleValueCoordList, FFReferences.PBoardCardTypeReferenceList, FFReferences.PBoardCardValueReferenceList);

            _cardDistance = new Tuple<int, int>(resultList[0].Item2, resultList[0].Item3);

            return resultList[0].Item1;
        }

        public virtual CCard GetRiver(Bitmap _bmp)
        {
            var temp = new Tuple<int, int>(0, 0);
            return GetRiver(_bmp, ref temp);
        }

        public override CCard GetRiver()
        {
            UpdateCurrentImage();
            return GetRiver(PCurrentBmp);
        }
        #endregion

        #endregion

        #region Numbers


        #region Unimplemented

        public override decimal GetPot(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }

        public override decimal GetPlayerBet(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }

        public override decimal GetPlayerStack(CPokerPositionModel.TenMax _playerPos)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region PlayerBet
        public decimal GetPlayerBet(PlayerPosition _pos, Bitmap _bmp)
        {
            var sampleCoordList = new List<Point>();

            Rectangle playerBetRegion = new Rectangle();
            switch (_pos)
            {
                case PlayerPosition.P0:
                    playerBetRegion = FFConstants.PLAYER_0_BET;
                    break;
                case PlayerPosition.P1:
                    playerBetRegion = FFConstants.PLAYER_1_BET;
                    break;
                default:
                    throw new Exception("Invalid Player position");
            }

            for (int i = 0; i < playerBetRegion.Width - FFConstants.BET_NUMBER_DIMENSION.Width; i++)
            {
                sampleCoordList.Add(new Point(
                    playerBetRegion.X + i,
                    playerBetRegion.Y));
            }

            var ret = GetNumbers(_bmp, sampleCoordList, FFReferences.PBetCharacterReferenceList, FFConstants.BET_NUMBER_DIMENSION);

            Console.WriteLine("Vilain Bet is: " + ret.ToString());

            return ret;
        }

        public decimal GetPlayerBet(PlayerPosition _pos)
        {
            UpdateCurrentImage(); 
            return GetPlayerBet(_pos, PCurrentBmp);
        }
        #endregion

        #region TotalPot
        public decimal GetPot(Bitmap _bmp)
        {
            var sampleCoordList = new List<Point>();

            for (int i = 0; i < FFConstants.POT.Width - FFConstants.POT_NUMBER_DIMENSION.Width; i++)
            {
                sampleCoordList.Add(new Point(
                    FFConstants.POT.X + i,
                    FFConstants.POT.Y));
            }

            var ret = GetNumbers(_bmp, sampleCoordList, FFReferences.PBetCharacterReferenceList, FFConstants.POT_NUMBER_DIMENSION);

            Console.WriteLine("Pot is: " + ret.ToString());

            return ret;
        }

        public decimal GetPot()
        {
            UpdateCurrentImage();
            return GetPot(PCurrentBmp);
        }
        #endregion

        #region PlayerStack

        public decimal GetPlayerStack(PlayerPosition _pos, Bitmap _bmp)
        {
            var sampleCoordList = new List<Point>();

            Rectangle playerStackRegion = new Rectangle();
            switch (_pos)
            {
                case PlayerPosition.P0:
                    playerStackRegion = FFConstants.PLAYER_0_STACK;
                    break;
                case PlayerPosition.P1:
                    playerStackRegion = FFConstants.PLAYER_1_STACK;
                    break;
                default:
                    throw new Exception("Invalid Player position");
            }

            for (int i = 0; i < playerStackRegion.Width - FFConstants.STACK_NUMBER_DIMENSION.Width; i++)
            {
                sampleCoordList.Add(new Point(
                playerStackRegion.X + i,
                playerStackRegion.Y));
            }

            var ret = GetNumbers(_bmp, sampleCoordList, FFReferences.PStackCharacterReferenceList, FFConstants.STACK_NUMBER_DIMENSION);

            Console.WriteLine("Stack is: " + ret.ToString());

            return ret;
        }

        public decimal GetPlayerStack(PlayerPosition _pos)
        {
            UpdateCurrentImage();
            return GetPlayerStack(_pos, PCurrentBmp);
        }
        #endregion

        #endregion

        #region Events

        #region NewRiverCardEvent

        protected bool FFMMIsRiverNew = true;

        public delegate void NewRiverEventHandler(CTableReaderTwoMax sender, RiverEventArgs e);
        public event NewRiverEventHandler ENewRiverCard;
        protected void MonitorRiverCard(Bitmap _bmp)
        {
            if (ENewRiverCard != null)
            {
                var dist = new Tuple<int, int>(0, 0);
                var river = GetRiver(_bmp, ref dist);
                if ((dist.Item1 < 1000000) && (dist.Item2 < 1000000))
                {
                    if (FFMMIsRiverNew)
                    {
                        Console.WriteLine("River Card Event Fired");
                        FFMMIsRiverNew = false;
                        var riverEventArgs = new RiverEventArgs();
                        riverEventArgs.PHwnd = PHwnd;
                        riverEventArgs.PRiver = river;
                        ENewRiverCard?.Invoke(this, riverEventArgs);
                    }
                }
                else
                {
                    FFMMIsRiverNew = true;
                }
            }
        }
        #endregion

        #region OurTurnEvent

        protected bool FFMMIsNewTurn = true;

        public delegate void OurTurnEventHandler(CTableReaderTwoMax sender, OurTurnEventArgs e);
        public event OurTurnEventHandler EOurTurn;
        protected void MonitorOurTurn(Bitmap _bmp)
        {
            if (EOurTurn != null)
            {

                var isOurTurn = IsOurTurn(_bmp);
                if (isOurTurn)
                {
                    if (FFMMIsNewTurn)
                    {
                        Console.WriteLine("OurTurnEvent fired");
                        FFMMIsNewTurn = false;
                        var ourTurnEventArgs = new OurTurnEventArgs();
                        ourTurnEventArgs.PBmp = _bmp;


                        PlayerModel leftPlayer = new PlayerModel();
                        PlayerModel rightPlayer = new PlayerModel();
                        if (IsHeroDealer(_bmp))
                        {
                            rightPlayer.Position = (CPokerPositionModel.TenMax)CPokerPositionModel.TwoMax.SB;
                            leftPlayer.Position = (CPokerPositionModel.TenMax)CPokerPositionModel.TwoMax.BB;
                        }
                        else
                        {
                            rightPlayer.Position = (CPokerPositionModel.TenMax)CPokerPositionModel.TwoMax.BB;
                            rightPlayer.Position = (CPokerPositionModel.TenMax)CPokerPositionModel.TwoMax.SB;
                        }

                        rightPlayer.Bet = GetPlayerBet(PlayerPosition.P1, _bmp);
                        leftPlayer.Bet = GetPlayerBet(PlayerPosition.P0, _bmp);

                        rightPlayer.Stack = GetPlayerStack(PlayerPosition.P1, _bmp);
                        leftPlayer.Stack = GetPlayerStack(PlayerPosition.P0, _bmp);

                        ourTurnEventArgs.PTotalPot = GetPot(_bmp);
                        ourTurnEventArgs.PPlayerList = new List<PlayerModel>();
                        ourTurnEventArgs.PPlayerList.Add(leftPlayer);
                        ourTurnEventArgs.PPlayerList.Add(rightPlayer);
                        EOurTurn?.Invoke(this, ourTurnEventArgs);
                    }
                }
                else
                {
                    FFMMIsNewTurn = true;
                }
            }
            else
            {
                FFMMIsNewTurn = true;
            }
        }
        #endregion

        #region PlayerNumberChanged

        private int FFMMPlayerCount = 0;
        static int qwe = 0;
        public delegate void PlayerCountEventHandler(CTableReaderTwoMax sender, PlayerCountEventArgs e);
        public event PlayerCountEventHandler EPlayerCountChange;
        protected void MonitorPlayerCount(Bitmap _bmp)
        {
            if (EPlayerCountChange != null)
            {
                var players = GetSeatedPlayers(_bmp);
                if (qwe++ % 10 == 0)
                {
                    _bmp.Save(@"C:\Users\admin\Desktop\ReferenceBMP\MonitoringSampleV2\b" + DateTime.UtcNow.Ticks.ToString() + ".bmp");
                }
                int i = 1;
                int nbPlayers = 0;
                while (i <= (int)PlayerPosition.P1)
                {
                    if ((players & (PlayerPosition)i) != PlayerPosition.None)
                    {
                        nbPlayers++;
                    }
                    i = i << 1;
                }
                //Console.WriteLine("**** PLAYER NUMBER : {0} *******", nbPlayers);
                if (nbPlayers != FFMMPlayerCount)
                {

                    FFMMPlayerCount = nbPlayers;
                    var playerCountEventArg = new PlayerCountEventArgs();
                    playerCountEventArg.PPlayerCount = (uint)nbPlayers;
                    playerCountEventArg.PHwnd = PHwnd;
                    EPlayerCountChange?.Invoke(this, playerCountEventArg);
                }
            }
        }
        #endregion

        protected override void InitializeMonitoringMembers()
        {
            FFMMIsNewTurn = true;
            FFMMIsRiverNew = true;
            FFMMPlayerCount = 0;
        }

        #endregion

    }
}
