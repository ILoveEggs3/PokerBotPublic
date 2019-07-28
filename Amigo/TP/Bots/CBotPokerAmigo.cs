using System;
using System.Collections.Generic;
using HoldemHand;
using Amigo.Helpers;
using Amigo.Controllers;
using System.Linq;
using Shared.Poker.Models;
using static Shared.Poker.Models.CAction;
using Shared.Helpers;
using static Shared.Poker.Models.CPlayer;
using Shared.Models;
using static Shared.Poker.Models.CTableInfos;
using Shared.Poker.Helpers;
using Shared.Models.Database;
using System.Threading;
using static Shared.Models.Database.CBoardModel;
using static HoldemHand.Hand;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Amigo.Models.MyModels.GameState;

namespace Amigo.Bots
{
    /// <summary>
    /// Equivalent of static class but is not a static class because it will be used for multi threading (Not true anymore?)
    /// </summary>
    public class CBotPokerAmigo : CBotPoker
    {
        private List<(ulong, double)> FFLstCurrentRange;
        private bool FFAlreadyUpdatedFlop;
        private bool FFAlreadyUpdatedTurn;
        private bool FFAlreadyUpdatedRiver;
        private ulong? FFTurnBoardMask;

        enum handType
        {
            Bluff,
            BluffEqu,
            SD,
            FD,
            SDFD,
            Value
        }

        enum handTypeRiver
        {
            Bluff,
            BluffEqu,
            Value,
            Blockers
        }

        public CBotPokerAmigo()
        {
            FFLstCurrentRange = null;
            FFAlreadyUpdatedFlop = false;
            FFAlreadyUpdatedTurn = false;
            FFAlreadyUpdatedRiver = false;
            FFTurnBoardMask = null;
        }        

        /// <summary>
        /// Gets the best EV+ decision to do from the current state of game.
        /// </summary>
        /// <param name="_headsUpTable">Informations that are necessary in order to receive an accurate decision from the bot.</param>
        /// <returns>Returns the best EV+ decision.</returns>
        public override CAction GetDecision(AState _currentGameState, Hand _heroHand, int _indexPlayerThatIsPlaying)
        {
            CAction finalDecision = null;
            CPlayer hero = _currentGameState.GetHeroPlayer();
            bool isHandShortStack = _currentGameState.PIsHandShortStack;

            #region Méthodes
            void MakeDecisionBetween2BBAnd3BBOpen(double _openSize)
            {
                string preflopVillainRange = null;

                if (_openSize == 3)
                    preflopVillainRange = "22+ A2s+ K2s+ Q7s+ J7s+ T7s+ 96s+ 85s+ 74s+ 64s+ 32s+ A2o+ K8o+ Q9o+ J8o+ 98o+";
                else
                    preflopVillainRange = "22+ A2s+ K2s+ Q2s+ J2s+ T4s+ 94s+ 84s+ 74s+ 63s+ 32s+ A2o+ K6o+ Q6o+ J6o+ 67o+";

                double preflopEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false, isHandShortStack);
                double chipsThatWeNeedToPut = (_openSize - _currentGameState.GetBBPlayer().PLastBet);
                double potOdds = Math.Round((chipsThatWeNeedToPut / (_currentGameState.PPot + chipsThatWeNeedToPut)) * 100, 2);

                if (preflopEquity > potOdds)                
                    finalDecision = Call();                
                else                
                    finalDecision = Fold();                
            }
            void MakeDecisionBetween2BBAnd8BBOpen(double _openSize)
            {            
                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AKo AKs"))
                {
                    finalDecision = new CAction(PokerAction.Raise, (_openSize * 4.3));
                }
                else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "99 TT AQo AQs AJs A2s A3s A4s A5s JTs+ KJs"))
                {
                    int randomNumber = CRandomNumberHelper.Between(0, 100);

                    // 30% of the time, we'll 3bet JTs+ KJs AJs-AQs AQo 99-TT
                    if (randomNumber >= 70)
                        finalDecision = new CAction(PokerAction.Raise, (_openSize * 4.3));
                    else
                        finalDecision = Call();
                }
                else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "45s 56s 67s 78s 89s 9Ts 75s 64s 53s A8s A7s A9s A6s KTs Q9s Q8s J9s QTs K5s Q5s KQo A5o"))
                {
                    int randomNumber = CRandomNumberHelper.Between(0, 100);

                    // 10% of the time, we'll 3bet the bluff range
                    if (randomNumber >= 90)
                        finalDecision = new CAction(PokerAction.Raise, (_openSize * 4.3));
                    else
                    {
                        #region Calling range
                        string preflopVillainRange = null;

                        if (_openSize >= 5)
                            preflopVillainRange = "22+ A2s+ KTs+ QTs+ J9s+ T9s 98s 87s 76s 65s 32s A2o+ KTo+ QJo";
                        else if (_openSize >= 3)
                            preflopVillainRange = "22+ A2s+ K2s+ Q7s+ J7s+ T7s+ 96s+ 85s+ 74s+ 64s+ 32s+ A2o+ K8o+ Q9o+ J8o+ 98o+";
                        else
                            preflopVillainRange = "22+ A2s+ K2s+ Q2s+ J2s+ T4s+ 94s+ 84s+ 74s+ 63s+ 32s+ A2o+ K6o+ Q6o+ J6o+ 67o+";

                        double preflopEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false, isHandShortStack);
                        double chipsThatWeNeedToPut = (_openSize - hero.PLastBet);
                        double potOdds = Math.Round((chipsThatWeNeedToPut / (_currentGameState.PPot + chipsThatWeNeedToPut)) * 100, 2);

                        if (preflopEquity > potOdds)                        
                            finalDecision = Call();                        
                        else                        
                            finalDecision = Fold();                        
                        #endregion
                    }
                }
                else
                {
                    #region Calling range
                    string preflopVillainRange = null;

                    if (_openSize >= 5)
                        preflopVillainRange = "22+ A2s+ KTs+ QTs+ J9s+ T9s 98s 87s 76s 65s 32s A2o+ KTo+ QJo";
                    else if (_openSize >= 3)
                        preflopVillainRange = "22+ A2s+ K2s+ Q7s+ J7s+ T7s+ 96s+ 85s+ 74s+ 64s+ 32s+ A2o+ K8o+ Q9o+ J8o+ 98o+";
                    else
                        preflopVillainRange = "22+ A2s+ K2s+ Q2s+ J2s+ T4s+ 94s+ 84s+ 74s+ 63s+ 32s+ A2o+ K6o+ Q6o+ J6o+ 67o+";

                    double preflopEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false, isHandShortStack);
                    double chipsThatWeNeedToPut = (_openSize - hero.PLastBet);
                    double potOdds = Math.Round((chipsThatWeNeedToPut / (_currentGameState.PPot + chipsThatWeNeedToPut)) * 100, 2);

                    if (preflopEquity > potOdds)                    
                        finalDecision = Call();                    
                    else                    
                        finalDecision = Fold();                    
                    #endregion
                }

            }
            CAction Check()
            {
                return new CAction(PokerAction.Check);
            }
            CAction Call()
            {
                return new CAction(PokerAction.Call, _currentGameState.PLastBet);
            }
            CAction Raise(double _amount)
            {
                return new CAction(PokerAction.Raise, _amount);
            }
            CAction Fold()
            {
                return new CAction(PokerAction.Fold);
            }
            CAction RaiseAllIn()
            {
                if (_currentGameState.GetLstAllowedActionsForCurrentPlayer().Contains(PokerAction.Raise))
                    return Raise(hero.PNumberOfChipsLeft + hero.PLastBet);
                else
                    return Call();
            }
            #endregion

            FFTimer.Restart();

            double heroStackInBB = hero.PNumberOfChipsAtBeginningHand.ToBB(_currentGameState.PBigBlind);
            double heroRoundedStackInBB = Math.Round(heroStackInBB, 0, MidpointRounding.AwayFromZero);
            

            #region Get a decision from the bot
            switch (_currentGameState.PCurrentStreet)
            {
                case Street.Preflop:
                    #region Preflop
                    switch (_currentGameState.PTypePot)
                    {
                        case TypesPot.Limped:
                            #region BTN limps. We're from BB.
                            if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "88+ ATs+ KJs+ QJs AJo+ KQo QJo"))
                            {
                                if (heroRoundedStackInBB <= 18)
                                    finalDecision = RaiseAllIn();
                                else
                                    finalDecision = Raise(_currentGameState.PBigBlind * 5);
                            }
                            else
                            {
                                if (heroRoundedStackInBB <= 18)
                                    finalDecision = Check();
                                else
                                {
                                    int randomNumber = CRandomNumberHelper.Between(0, 100);

                                    if (randomNumber >= 70)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "66+ A2s+ K8s+ Q9s+ J9s+ T8s+ 97s+ 87s 76s 65s 54s ATo+ KJo+ QJo"))
                                            finalDecision = Raise(_currentGameState.PBigBlind * 5);
                                        else
                                            finalDecision = Check();
                                    }
                                }
                            }
                            #endregion
                            break;
                        case TypesPot.OneBet:
                            #region We're from BTN. Nothing happened.
                                
                            #region If effectives stacks are <= 18BB
                            if (heroRoundedStackInBB <= 18)
                            {
                                switch (heroRoundedStackInBB)
                                {
                                    case 1:
                                    case 2:
                                        finalDecision = RaiseAllIn(); // Jam any two cards. YOLO.
                                        break;
                                    case 3:
                                    case 4:
                                    case 5:
                                    case 6:
                                    case 7:
                                    case 8:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q3o+ J2s+ J7o+ T4s+ T7o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 9:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q5o+ J2s+ J8o+ T5s+ T8o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 10:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q7o+ J3s+ J8o+ T5s+ T8o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 11:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q8o+ J4s+ J8o+ T5s+ T8o+ 95s+ 98o 85s+ 87o 74s+ 64s+ 53s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 12:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K3o+ Q2s+ Q8o+ J5s+ J9o+ T6s+ T8o+ 95s+ 98o 85s+ 87o 75s+ 64s+ 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 13:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K4o+ Q3s+ Q8o+ J5s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 64s+ 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 14:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K5o+ Q4s+ Q9o+ J5s+ J8o+ T6s+ T8o+ 95s+ 98o 85s+ 87o 75s+ 64s+ 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 15:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K6o+ Q4s+ Q9o+ J6s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 64s+ 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 16:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K7o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T9o 96s+ 98o 85s+ 75s+ 65s 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 17:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K8o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 65s 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    case 18:
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ A2o+ A2s+ K2s+ K8o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T9o 96s+ 98o 85s+ 75s+ 65s 54s"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                        break;
                                    default:
                                        throw new Exception("EffectivesStacks <= 18, but we did not managed to find an open shoving range... This situation should never happen!");
                                }
                            }
                            #endregion
                            else 
                            {
                                string openRange = "22+ A2s+ K2s+ Q2s+ J2s+ T2s+ 92s+ 82s+ 74s+ 63s+ 52s+ 42s+ 32s A2o+ K2o+ Q4o+ J5o+ T6o+ 97o+ 86o+ 76o";

                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, openRange))
                                {
                                    if (heroStackInBB <= 40)
                                        finalDecision = new CAction(PokerAction.Raise, 2);
                                    else
                                        finalDecision = new CAction(PokerAction.Raise, 3);
                                }
                                else
                                    finalDecision = Fold();
                            }
                            #endregion
                            break;
                        case TypesPot.TwoBet:
                            #region We're on the BB. BTN opened.                            
                            double openSize = _currentGameState.PLastBet;

                            if (heroRoundedStackInBB <= 0)
                                throw new Exception("heroRoundedStackInBB is smaller than 0BB. This is not normal! The bot cannot take a decision.");
                            else if (openSize < 2)
                                throw new Exception("Cannot detect the open of the preflop raiser! Open size was less than 2BB! The bot cannot take a decision.");

                            if (heroStackInBB <= 70)
                            {
                                    
                                if (heroStackInBB <= 20)
                                {
                                    if (openSize <= 3)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "66+ ATo+ KJs+ A8s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            MakeDecisionBetween2BBAnd3BBOpen(openSize);
                                    }
                                    else
                                    {
                                        bool raiseAllIn = false;

                                        if (heroStackInBB <= 8)                                            
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ K6o+ K2s+ A2o+ A2s+ QJo Q6s+"));
                                        else if (heroStackInBB == 9)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ K8o+ K2s+ A2o+ A2s+ QJo Q8s+"));
                                        else if (heroStackInBB == 10)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ KTo+ K3s+ A2o+ A2s+ Q9s+"));
                                        else if (heroStackInBB == 11)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ KTo+ K5s+ A2o+ A2s+ QTs+"));
                                        else if (heroStackInBB == 12)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ KJo+ K7s+ A2o+ A2s+ QJs"));
                                        else if (heroStackInBB == 13)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "22+ KJo+ K8s+ A2o+ A2s+"));
                                        else if (heroStackInBB == 14)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "33+ KQo K9s+ A2o+ A2s+"));
                                        else if (heroStackInBB == 15)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "44+ KQo KTs+ A3o+ A2s+"));
                                        else if (heroStackInBB == 16)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "44+ KQo KJs+ A4o+ A2s+"));
                                        else if (heroStackInBB == 17)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "44+ KJs+ A4o+ A2s+"));
                                        else if (heroStackInBB <= 20)
                                            raiseAllIn = (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "44+ KQs A5o+ A2s+"));

                                        if (raiseAllIn)
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                }
                                #region Stacks are between 21BB and 70BB
                                else if (heroStackInBB <= 30)
                                {
                                    #region Stack is <= 30BB
                                    if (openSize <= 3)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "66+ ATo+ KJs+ A8s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            MakeDecisionBetween2BBAnd3BBOpen(openSize);
                                    }
                                    else
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "66+ A6o+ KJs+ A4s+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region Stack is > 30BB and <= 70BB
                                    if (openSize >= 8)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "KJs+ AJs+ AJo+ 99+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }                                            
                                    else
                                        MakeDecisionBetween2BBAnd8BBOpen(openSize);
                                    #endregion
                                }

                                #endregion
                            }
                            else if (heroStackInBB <= 115)
                            {
                                #region Stacks are between 71BB and 115BB
                                #region If the open is >= 8BB
                                if (openSize >= 8)
                                {
                                    if (openSize <= 20)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "KJs+ AJs+ AJo+ 99+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                    else
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AJs+ AJo+ TT+"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                }
                                #endregion
                                else                                    
                                    MakeDecisionBetween2BBAnd8BBOpen(openSize); // If the open is >= 2BB and < 8BB     
                                #endregion
                            }
                            else
                            {
                                #region Stacks are over 115BB

                                if (openSize >= 8)
                                {
                                    #region If the open is >= 8BB and < 25BB
                                    if (openSize < 25)
                                    {
                                        string range = null;

                                        if (heroStackInBB >= 200)
                                            range = "AQo+ KJs+ ATs+ 88+";
                                        else
                                            range = "AQo+ AJs+ TT+";

                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, range))
                                            finalDecision = Call();
                                        else
                                            finalDecision = Fold();
                                    }
                                    #endregion
                                    #region If the open is higher than 25BB
                                    else
                                    {
                                        string range = null;

                                        if (heroStackInBB >= 200)
                                            range = "AKo+ AKs+ KK+";
                                        else
                                            range = "AKo+ AKs+ JJ+";

                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, range))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                    #endregion
                                }
                                else                                                                            
                                    MakeDecisionBetween2BBAnd8BBOpen(openSize); // If the open is >= 2BB and < 8BB                                    

                                #endregion
                            }
                            #endregion
                            break;
                        case TypesPot.ThreeBet:
                            #region We're from BTN. BB 3bet us.
                            double heroOpenSize = hero.PLastBet;
                            double villain3BetSize = _currentGameState.GetVilainPlayer().PLastBet.ToBB(_currentGameState.PBigBlind);
                            string villain3BetRange = "TT+ AJs+ KJs+ QJs JTs T9s 98s 87s 76s 65s AQo+";

                            if (heroStackInBB <= 30)
                            {
                                #region Effective stacks are <= 30BB
                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "99+ ATo+ KQs+ ATs+"))
                                    finalDecision = RaiseAllIn();
                                else if (villain3BetSize < 9)
                                {
                                    double preflopEquity30BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false, isHandShortStack);
                                    double chipsThatWeNeedToPut30BB = (villain3BetSize - heroOpenSize);
                                    double potOdds30BB = Math.Round((chipsThatWeNeedToPut30BB / (_currentGameState.PPot + chipsThatWeNeedToPut30BB)) * 100, 2);

                                    if (preflopEquity30BB > potOdds30BB)                                        
                                        finalDecision = Call();                                        
                                    else                                        
                                        finalDecision = Fold();                                        
                                }
                                else
                                    finalDecision = Fold();
                                #endregion
                            }
                            else if (heroStackInBB <= 60)
                            {
                                #region Effective stacks are <= 60BB
                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AQs+ AQo+"))
                                    finalDecision = RaiseAllIn();
                                else if (villain3BetSize <= 13)
                                {
                                    double preflopEquity60BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false, isHandShortStack);
                                    double chipsThatWeNeedToPut60BB = (villain3BetSize - heroOpenSize);
                                    double potOdds60BB = Math.Round((chipsThatWeNeedToPut60BB / (_currentGameState.PPot + chipsThatWeNeedToPut60BB)) * 100, 2);

                                    if (preflopEquity60BB > potOdds60BB)                                        
                                        finalDecision = Call();                                        
                                    else                                        
                                        finalDecision = Fold();                                        
                                }
                                else
                                    finalDecision = Fold();
                                #endregion
                            }
                            else if (heroStackInBB <= 120)
                            {
                                #region Effective stacks are <= 120BB
                                bool normal3BetSize = (villain3BetSize <= 13);

                                if (normal3BetSize)
                                {
                                    if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AKo AKs"))
                                    {
                                        #region 4bet range 70% of the time (value only)
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 70% of the time, we'll 4bet JJ+ AK+
                                        if (randomNumber >= 30)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 2.25));
                                        else
                                            finalDecision = Call();
                                        #endregion
                                    }
                                    else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "KQs AJs"))
                                    {
                                        #region 4bet range 20% of the time (value/bluff sometimes)
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 20% of the time, we'll 4bet KQs AJs
                                        if (randomNumber >= 80)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 2.25));
                                        else
                                            finalDecision = Call();
                                        #endregion
                                    }
                                    else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "67s 68s 97s A2s A3s A4s A5s 98s 78s 75s 56s AQo A5o KQo"))
                                    {
                                        #region 4bet range 10% of the time (bluffs)
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 10% of the time, we'll 4bet 67s 68s 97s A2s-A5s 98s 78s 75s 56s AQo A5o KQo
                                        if (randomNumber >= 90)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 2.25));
                                        else
                                        {
                                            double preflopEquity120BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), true, isHandShortStack);
                                            double chipsThatWeNeedToPut120BB = (villain3BetSize - heroOpenSize);
                                            double potOdds120BB = Math.Round((chipsThatWeNeedToPut120BB / (_currentGameState.PPot + chipsThatWeNeedToPut120BB)) * 100, 2);                                            

                                            if (preflopEquity120BB > potOdds120BB)                                                
                                                finalDecision = Call();                                                
                                            else                                                
                                                finalDecision = Fold();                                                
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Not in the 4bet range. Whatever hand we got, we calculate odds before calling.
                                        double preflopEquity120BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), true, isHandShortStack);
                                        double chipsThatWeNeedToPut120BB = (villain3BetSize - heroOpenSize);
                                        double potOdds120BB = Math.Round((chipsThatWeNeedToPut120BB / (_currentGameState.PPot + chipsThatWeNeedToPut120BB)) * 100, 2);

                                        if (preflopEquity120BB > potOdds120BB)                                            
                                            finalDecision = Call();                                            
                                        else                                            
                                            finalDecision = Fold();                                            
                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region 4bet range only in value since very high sizing
                                    if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AKo AKs"))
                                        finalDecision = RaiseAllIn();
                                    else                                        
                                        finalDecision = Fold();                                        
                                    #endregion
                                }
                                #endregion
                            }
                            else
                            {
                                #region Effective stacks are > 120BB
                                bool normal3BetSize = (villain3BetSize <= 13);

                                if (normal3BetSize)
                                {
                                    if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AKo AKs"))
                                    {
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 70% of the time, we'll 4bet JJ+ AK+
                                        if (randomNumber >= 30)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 3));
                                        else
                                            finalDecision = Call();
                                    }
                                    else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "KQs AJs"))
                                    {
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 20% of the time, we'll 4bet KQs AJs
                                        if (randomNumber >= 80)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 3));
                                        else
                                            finalDecision = Call();
                                    }
                                    else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "67s 68s 97s A2s A3s A4s A5s 98s 78s 75s 56s AQo A5o KQo"))
                                    {
                                        int randomNumber = CRandomNumberHelper.Between(0, 100);

                                        // 10% of the time, we'll 4bet 67s 68s 97s A2s-A5s 98s 78s 75s 56s AQo A5o KQo
                                        if (randomNumber >= 90)
                                            finalDecision = new CAction(PokerAction.Raise, (villain3BetSize * 3));
                                        else
                                        {
                                            double preflopEquityHigher120BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), true, isHandShortStack);
                                            double chipsThatWeNeedToPutHigher120BB = (villain3BetSize - heroOpenSize);
                                            double potOddsHigher120BB = Math.Round((chipsThatWeNeedToPutHigher120BB / (_currentGameState.PPot + chipsThatWeNeedToPutHigher120BB)) * 100, 2);

                                            if (preflopEquityHigher120BB > potOddsHigher120BB)                                                
                                                finalDecision = Call();                                                
                                            else                                                
                                                finalDecision = Fold();                                                
                                        }
                                    }
                                    else
                                    {
                                        double preflopEquityHigher120BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), true, isHandShortStack);
                                        double chipsThatWeNeedToPutHigher120BB = (villain3BetSize - heroOpenSize);
                                        double potOddsHigher120BB = Math.Round((chipsThatWeNeedToPutHigher120BB / (_currentGameState.PPot + chipsThatWeNeedToPutHigher120BB)) * 100, 2);

                                        if (preflopEquityHigher120BB > potOddsHigher120BB)                                            
                                            finalDecision = Call();                                            
                                        else                                            
                                            finalDecision = Fold();                                            
                                    }
                                }
                                else
                                {
                                    if (villain3BetSize < 30)
                                    {
                                        if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AA"))
                                        {
                                            int randomNumber = CRandomNumberHelper.Between(0, 100);

                                            // 70% of the time, we'll 4bet jam only AA
                                            if (randomNumber >= 30)
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Call();
                                        }
                                        else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "JJ+ AKo AKs"))
                                            finalDecision = Call();
                                        else
                                            finalDecision = Fold();
                                    }
                                    else
                                    {
                                        if (heroStackInBB <= 200)
                                        {
                                            if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AKo AKs QQ+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                        }
                                        else if (heroStackInBB <= 300)
                                        {
                                            if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "KK+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                        }
                                        else if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AA"))
                                            finalDecision = RaiseAllIn();
                                        else
                                            finalDecision = Fold();
                                    }
                                }
                                #endregion
                            }
                            #endregion
                            break;
                        case TypesPot.LimpedThreeBet:
                        case TypesPot.FourBet:
                            #region We're from BB. BTN 4bets us.
                            double heroThreeBetSize = hero.PLastBet;
                            double villain4BetSize = _currentGameState.GetVilainPlayer().PLastBet;
                                
                            if (heroStackInBB <= 80)
                            {
                                #region Stacks are shorter than 80BB
                                string preflop80BBVillainRange = "TT+ AKo+ AKs+";

                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "TT+ AQo+ AQs+"))
                                    RaiseAllIn();
                                else
                                {
                                    double preflop80BBEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflop80BBVillainRange), false, isHandShortStack);
                                    double chipsThatWeNeedToPut80BB = (villain4BetSize - hero.PLastBet);
                                    double potOdds80BB = Math.Round((chipsThatWeNeedToPut80BB / (_currentGameState.PPot + chipsThatWeNeedToPut80BB)) * 100, 2);

                                    if (preflop80BBEquity > potOdds80BB)
                                        finalDecision = Call();
                                    else
                                        finalDecision = Fold();
                                }
                                #endregion
                            }
                            else if (heroStackInBB <= 120)
                            {
                                #region Stacks are > 80BB and <= 120BB
                                string preflopVillainRange120BB = "TT+ AQo+ AQs+";

                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "QQ+ AKo+ AKs+"))
                                {
                                    int randomNumber = CRandomNumberHelper.Between(0, 100);

                                    // 30% of the time, we'll jam QQ+ AK+
                                    if (randomNumber >= 70)
                                        finalDecision = RaiseAllIn();
                                    else
                                        finalDecision = Call();
                                }
                                else
                                {
                                    double preflopEquity120BB = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange120BB), false, isHandShortStack);
                                    double chipsThatWeNeedToPut120BB = (villain4BetSize - hero.PLastBet);
                                    double potOdds120BB = Math.Round((chipsThatWeNeedToPut120BB / (_currentGameState.PPot + chipsThatWeNeedToPut120BB)) * 100, 2);

                                    if (preflopEquity120BB > potOdds120BB)
                                        finalDecision = Call();
                                    else
                                        finalDecision = Fold();
                                }
                                #endregion
                            }
                            else
                            {
                                #region Stacks are > 120BB
                                if (CPokerRangeConverter.GetInstance().isOurHandInThisRange(_heroHand, "AA"))
                                {
                                    int randomNumber = CRandomNumberHelper.Between(0, 100);

                                    // 10% of the time we jam AA 120BB deep +
                                    if (randomNumber >= 90)
                                        finalDecision = RaiseAllIn();
                                    else
                                        finalDecision = Call();
                                }
                                else
                                {
                                    string preflopHigher120BBVillainRange = "TT+ AQo+ AQs+";

                                    double preflopHigher120BBEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopHigher120BBVillainRange), false, isHandShortStack);
                                    double chipsThatWeNeedToPutHigher120BB = (villain4BetSize - hero.PLastBet);
                                    double potOddsHigher120BB = Math.Round((chipsThatWeNeedToPutHigher120BB / (_currentGameState.PPot + chipsThatWeNeedToPutHigher120BB)) * 100, 2);

                                    if (preflopHigher120BBEquity > potOddsHigher120BB)
                                        finalDecision = Call();
                                    else
                                        finalDecision = Fold();
                                }
                                #endregion
                            }
                            #endregion
                            break;
                        case TypesPot.FiveBetEtPlus:
                        case TypesPot.LimpedFourBetEtPlus:
                            #region We're from BTN. BB 5bets us.
                            double heroFourBetSize = hero.PLastBet;
                            double villain5BetSize = _currentGameState.GetVilainPlayer().PLastBet;

                            string preflopVillainRange = "JJ+ AKo+ AKs+";

                            if (_currentGameState.GetVilainPlayer().PNumberOfChipsLeft == 0)
                            {
                                string heroHandString = _heroHand.ToString();
                                double preflopRawEquity = CalculatePreflopRawEquity(heroHandString.Substring(0, 2).ToCCarte(),
                                                                                    heroHandString.Substring(3, 2).ToCCarte(),
                                                                                    CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange));
                                double chipsThatWeNeedToPut = (villain5BetSize - hero.PLastBet);
                                double potOdds = Math.Round((chipsThatWeNeedToPut / (_currentGameState.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                if (preflopRawEquity > potOdds)
                                    finalDecision = Call();
                                else
                                    finalDecision = Fold();
                            }
                            else
                            {
                                double preflopEquity = CalculatePreflopRealizedEquity(_heroHand, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), true, isHandShortStack);
                                double chipsThatWeNeedToPut = (villain5BetSize - hero.PLastBet);
                                double potOdds = Math.Round((chipsThatWeNeedToPut / (_currentGameState.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                if (preflopEquity > potOdds)
                                    finalDecision = Call();
                                else
                                    finalDecision = Fold();
                            }
                            #endregion
                            break;
                        default:
                            throw new InvalidOperationException("Invalid type pot!");
                    }
                    #endregion
                    break;
                case Street.Flop:
                    #region Flop decision
                    BoardMetaDataFlags flopBoardMetaData = CDBHelperHandInfos.PDicAllBoardsByBoardMask[_heroHand.BoardMask].Item2;

                    if (!FFAlreadyUpdatedFlop)
                    {
                        FFLstCurrentRange = GetRangePreflop(_currentGameState.PTypePot,
                                                            _currentGameState.GetVilainPlayerPosition(),
                                                            _heroHand.PocketMask,
                                                            _heroHand.BoardMask);

                        FFAlreadyUpdatedFlop = true;
                    }

                    if ((_currentGameState.PTypeFilteredPot == TypesPot.TwoBet) && (_currentGameState.GetLastActionStreet() == Street.Preflop))
                        finalDecision = Check();
                    else
                    {
                        FFLstCurrentRange = GetRangeFlop(_currentGameState.PGameState,
                                                         flopBoardMetaData,
                                                         FFLstCurrentRange,
                                                         _heroHand.BoardMask,
                                                         _indexPlayerThatIsPlaying);

                        var flopBestGameState = CPokerMath.GetEv(_currentGameState,
                                                                 FFLstCurrentRange,
                                                                 _heroHand.BoardMask,
                                                                 _heroHand.PocketMask,
                                                                 _indexPlayerThatIsPlaying
                                                                 );

                        finalDecision = flopBestGameState.Item1.GetLastAction();
                    }
                    #endregion
                    break;
                case Street.Turn:
                    #region Turn decision
                    BoardMetaDataFlags turnBoardMetaData = CDBHelperHandInfos.PDicAllBoardsByBoardMask[_heroHand.BoardMask].Item2;

                    FFTurnBoardMask = _heroHand.BoardMask;
                    if (!FFAlreadyUpdatedTurn)
                    {
                        FFLstCurrentRange = UpdateRangeWithNewBoard(FFLstCurrentRange, FFTurnBoardMask.Value, _indexPlayerThatIsPlaying);
                        FFAlreadyUpdatedTurn = true;
                    }

                    
                    var turnBestGameState = CPokerMath.GetEv(_currentGameState, FFLstCurrentRange, FFTurnBoardMask.Value, _heroHand.PocketMask, _indexPlayerThatIsPlaying);

                    finalDecision = turnBestGameState.Item1.GetLastAction();
                    #endregion
                    break;
                case Street.River:
                    #region River decision
                    BoardMetaDataFlags riverBoardMetaData = CDBHelperHandInfos.PDicAllBoardsByBoardMask[_heroHand.BoardMask].Item2;

                    if (!FFAlreadyUpdatedRiver)
                    {
                        FFLstCurrentRange = UpdateRangeWithNewBoard(FFLstCurrentRange, _heroHand.BoardMask, _indexPlayerThatIsPlaying);
                        FFAlreadyUpdatedTurn = true;
                    }

                    var riverBestGameState = CPokerMath.GetEv(_currentGameState, FFLstCurrentRange, _heroHand.BoardMask, _heroHand.PocketMask, _indexPlayerThatIsPlaying, FFTurnBoardMask);

                    finalDecision = riverBestGameState.Item1.GetLastAction(riverBestGameState.Item1.PPreviousTurnPlayerIndex);
                    #endregion
                    break;
            }
            #endregion

            FFTimer.Stop();
            Console.WriteLine("Decision taken in: " + FFTimer.ElapsedMilliseconds);

            return finalDecision;
        }

        public override void CreateNewHand()
        {
            FFLstCurrentRange = null;
            FFAlreadyUpdatedFlop = false;
            FFAlreadyUpdatedTurn = false;
            FFAlreadyUpdatedRiver = false;
            FFTurnBoardMask = null;
        }

        #region Preflop related functions
        /// <summary>
        /// Calculate preflop equity based on a range and a hand given
        /// </summary>
        /// <returns></returns>        
        private double CalculatePreflopRealizedEquity(Hand _heroCards, string[] _villainRange, bool _heroIsInPosition, bool _isHandShortStack)
        {
            const int MAXIMUM_TRIAL = 200000;

            long heroWins = 0;
            long ties = 0;
            long count = 0;

            Parallel.ForEach(_villainRange, (villainHand) =>
            {
                ulong villainMask = Hand.ParseHand(villainHand);

                if (_heroCards.PocketMask != villainMask)
                {
                    IEnumerable<ulong> enumBoardMasks = Hand.RandomHands(0L, _heroCards.PocketMask | villainMask, 5, MAXIMUM_TRIAL / _villainRange.Length);
                    Parallel.ForEach(enumBoardMasks, (boardMask) =>
                    {
                        uint heroHandValue = Hand.Evaluate(boardMask | _heroCards.PocketMask, 7);
                        uint villainHandValue = Hand.Evaluate(boardMask | villainMask, 7);

                        // Calculate Winners
                        if (heroHandValue > villainHandValue)
                            Interlocked.Increment(ref heroWins);
                        else if (heroHandValue == villainHandValue)
                            Interlocked.Increment(ref ties);

                        Interlocked.Increment(ref count);
                    });
                }
            });

            double preflopEquity = (Math.Round((((double)heroWins) + ((double)ties) / 2.0) / ((double)count) * 100.0, 2));

            if (!Hand.IsSuited(_heroCards.PocketMask))            
                preflopEquity -= 2;                        

            int gapCount = Hand.GapCount(_heroCards.PocketMask);

            if (gapCount == 0)
            {
                preflopEquity += 1.5;
            }
            else if (gapCount == 1)
            {
                preflopEquity += 0.7;
            }

            if (_heroIsInPosition)
            {
                preflopEquity += 2.5;
            }
            else
            {
                preflopEquity -= 2.5;
            }

            if (_isHandShortStack)
                preflopEquity -= 3;

            return preflopEquity;
        }

        private double CalculatePreflopRawEquity(CCard _heroCard1, CCard _heroCard2, string[] _villainRange)
        {
            const int MAXIMUM_TRIAL = 200000;

            FFTimer.Restart();

            string heroCard1 = _heroCard1.ToString();
            string heroCard2 = _heroCard2.ToString();

            long heroWins = 0;
            long ties = 0;
            long count = 0;

            ulong heroMask = Hand.ParseHand(heroCard1 + heroCard2);

            foreach (string villainCurrentHand in _villainRange)
            {
                string firstVillainCard = (villainCurrentHand.Substring(0, 2));
                string secondVillainCard = (villainCurrentHand.Substring(2, 2));

                if (!(heroCard1 == firstVillainCard || heroCard1 == secondVillainCard ||
                      heroCard2 == firstVillainCard || heroCard2 == secondVillainCard))
                {
                    ulong villainMask = Hand.ParseHand(villainCurrentHand);

                    IEnumerable<ulong> enumBoardMasks = Hand.RandomHands(0L, heroMask | villainMask, 5, ((double)MAXIMUM_TRIAL / _villainRange.Length));
                    foreach (ulong boardMask in enumBoardMasks)
                    {
                        uint heroHandValue = Hand.Evaluate(boardMask | heroMask, 7);
                        uint villainHandValue = Hand.Evaluate(boardMask | villainMask, 7);

                        // Calculate Winners
                        if (heroHandValue > villainHandValue)
                            ++heroWins;
                        else if (heroHandValue == villainHandValue)
                            ++ties;

                        ++count;
                    }
                }
            }

            double preflopEquity = (Math.Round((((double)heroWins) + ((double)ties) / 2.0) / ((double)count) * 100.0, 2));
            FFTimer.Stop();

            return preflopEquity;
        }
        #endregion
        #region Postflop related functions
        public static List<(ulong, double)> UpdateRange(List<(ulong, double)> _lstRange, ulong _cardMask)
        {
            if (_lstRange.Count == 0)
                return _lstRange;

            var lstRange = new List<(ulong, double)>(_lstRange.Count);

            lstRange = _lstRange.Where(x => (x.Item1 & _cardMask) == 0).ToList();
            var qwe = 0.0d;
            lstRange.ForEach(x => qwe += x.Item2);
            lstRange = lstRange.Select(x => (x.Item1, x.Item2 / qwe)).ToList();

            #if DEBUG
            double toto = 0;
            foreach (var tati in lstRange)
                toto += tati.Item2;

            if (((toto < 0.99) || (toto > 1.01)) && lstRange.Count != 0)
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
            #endif

            return lstRange;
        }
        public static List<(ulong, double)> UpdateRangeWithNewBoard(List<(ulong, double)> _lstRange, ulong _boardMask, int _indexPlayer)
        {
            if (_lstRange.Count == 0)
                return _lstRange;

            var lstRange = new List<(ulong, double)>(_lstRange.Count);
            double totalPercent = 1;
            object lockOperation = new object();

            foreach (var tupleInfos in _lstRange)
            {
                // If the current combo is in the board
                if (!CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask].ContainsKey(tupleInfos.Item1))
                    totalPercent -= tupleInfos.Item2;          
                else                
                    lstRange.Add((tupleInfos.Item1, tupleInfos.Item2));                
            }

            for (int handIndex = 0; handIndex < lstRange.Count; ++handIndex)
            {
                var tupleInfos = lstRange[handIndex];
                double newProbability = (tupleInfos.Item2 / totalPercent);

                lstRange[handIndex] = (tupleInfos.Item1, newProbability);
            }

            #if DEBUG
            double toto = 0;
            foreach (var tati in lstRange)
                toto += tati.Item2;

            if (((toto < 0.99) || (toto > 1.01)) && lstRange.Count != 0)
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
            #endif

            return lstRange;
        }

        public static List<(ulong, double)> GetRangePreflop(TypesPot _unfilteredTypePot, PokerPosition _villainPosition, ulong _heroPocketMask, ulong _boardMask)
        {
            var key = (_unfilteredTypePot, _villainPosition);
            var lstRange = new List<(ulong, double)>(1326);
            var handsThatWeCounted = new HashSet<ulong>();
            long totalSampleCount = (CDBHelper.PDicTotalPreflopSampleCount[key] - CDBHelper.PAveragePlayerPreflopRange[key][_heroPocketMask]); // Remove hero's card from total sample count

            handsThatWeCounted.Add(_heroPocketMask);

            foreach (var handOnBoard in Hand.Hands(0, ~(_boardMask | _heroPocketMask), 1))
            {
                foreach (var handRelatedToCard in CDBHelper.PAllHandsByCard[handOnBoard])
                {
                    if (!handsThatWeCounted.Contains(handRelatedToCard))
                    {
                        handsThatWeCounted.Add(handRelatedToCard);
                        totalSampleCount -= CDBHelper.PAveragePlayerPreflopRange[key][handRelatedToCard];
                    }
                }
            }

            foreach (var infos in CDBHelper.PAveragePlayerPreflopRange[key])
            {
                // If the current card is contained either the board or the hero pocket mask, we don't add. Otherwise, we add it to the new range     
                if (((infos.Key & _boardMask) == 0) && ((infos.Key & _heroPocketMask) == 0))
                    lstRange.Add((infos.Key, (infos.Value / (double)totalSampleCount)));
            }

            #if DEBUG
            double toto = 0;
            foreach (var tati in lstRange)
                toto += tati.Item2;

            if ((toto < 0.99) || (toto > 1.01))
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
            #endif

            return lstRange;
        }
        public static List<(ulong, double)> GetRangeFlop(long _flopGameStateID, BoardMetaDataFlags _boardType, List<(ulong, double)> _lstRange, ulong _boardMask, int _indexPlayer)
        {
            if (_lstRange.Count == 0)
                return _lstRange;

            long numberOfSample = CDBHelper.PAllRangeSamplesFlop[(_flopGameStateID, _boardType)];

            if (numberOfSample <= 0)
                return new List<(ulong, double)>();

            #region Initialization
            var dataBluff = new Dictionary<ulong, Dictionary<(bool, bool, bool, bool, byte), (double, double)>>();
            var data = new Dictionary<handType, Dictionary<ulong, (double, double)>>();
            var countT = new Dictionary<handType, double>();
            var key = (_flopGameStateID, _boardType);

            var countBluffRange = new Dictionary<(bool, bool, bool, bool, byte), double>(); // First item = Number of time we counted the range
            var bluffIndexDic = new Dictionary<ulong, HashSet<(bool, bool, bool, bool, byte)>>();

            var countBluffEquDic = new Dictionary<int, double>(); // This is used to verify a special condition only
            double countBluffEqu = 0;

            var countBluffFD = new Dictionary<int, double>();
            var FDDic = new Dictionary<ulong, byte>();

            var countHS = new Dictionary<double, double>();
            var HsDic = new Dictionary<ulong, double>();

            double countSDFD = 0;
            double countSD = 0;

            bool bluffSDAndFD = false;
            bool bluffSD = false;

            countT.Add(handType.Bluff, 0);

            countT.Add(handType.BluffEqu, 0);
            data.Add(handType.BluffEqu, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.Value, 0);
            data.Add(handType.Value, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.SD, 0);
            data.Add(handType.SD, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.FD, 0);
            data.Add(handType.FD, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.SDFD, 0);
            data.Add(handType.SDFD, new Dictionary<ulong, (double, double)>());

            List<(ulong, double)> lstRange = new List<(ulong, double)>(_lstRange.Count);
            #endregion

            #region Local methods
            void LFFilterData()
            {
                foreach (var tupleInfos in _lstRange)
                {
                    var pocketMask = tupleInfos.Item1;
                    if (Hand.GetHandTypeExcludingBoard(pocketMask, _boardMask) == Hand.HandTypes.HighCard)
                    {
                        #region Bluff hand
                        if (CDBHelper.PAveragePlayerBluffsFlop.ContainsKey(key))
                        {
                            var lstBluffs = CDBHelper.PAveragePlayerBluffsFlop[key];

                            // Insert the Flop state to the "to-do" list      
                            bool isBDFD = Hand.IsBackdoorFlushDraw(pocketMask, _boardMask, 0L);
                            bool isBDSD = Hand.IsBackDoorStraightDraw(pocketMask, _boardMask);
                            bool isSD = Hand.IsStraightDraw(pocketMask, _boardMask, 0L);
                            bool isFD = Hand.IsFlushDraw(pocketMask, _boardMask, 0L);
                            byte highestIndex = CBotPokerAmigo.CardIndex(pocketMask, _boardMask);

                            foreach (var info in lstBluffs.Where(x => x.Item1 == isBDFD && x.Item2 == isBDSD && x.Item3 == isSD && x.Item4 == isFD))
                            {
                                bool added = false;

                                #region If the current hand is a bluff valid, we add it to the dictionary associated
                                if (isFD)
                                {
                                    // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                                    if (isSD)
                                        added = true;                                    
                                    else if (highestIndex == info.Item5)                                                                            
                                        added = true; // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand                                                                            
                                }
                                // His hand was a straight draw, but not a flush draw
                                else if (isSD)
                                {
                                    // Could only be doing the action with straight draws + BDFD, so we only include hands that are str8 draws + BDFD in this case
                                    if (isBDFD)
                                    {
                                        if (highestIndex == info.Item5)
                                        {
                                            added = true;
                                        }
                                    }
                                    // In this case, villain probably do the action with all of his straight draws
                                    else
                                    {
                                        if (info.Item5 == 0)
                                        {
                                            if (highestIndex == 0)
                                            {
                                                added = true;
                                            }
                                        }
                                        else
                                        {
                                            if (highestIndex != 0) // Accept all hands that are > index 0
                                            {
                                                added = true;
                                            }
                                        }
                                    }
                                }
                                else if (isBDFD)
                                {
                                    if (isBDSD)
                                    {
                                        if (info.Item5 == 0)
                                        {
                                            if (highestIndex == 0)
                                            {
                                                added = true;
                                            }
                                        }
                                        else
                                        {
                                            if (highestIndex != 0)
                                            {
                                                added = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (highestIndex == info.Item5)
                                        {
                                            added = true;
                                        }
                                    }
                                }
                                else if (isBDSD)
                                {
                                    if (info.Item5 == 0)
                                    {
                                        if (highestIndex == 0)
                                        {
                                            added = true;
                                        }

                                    }
                                    else
                                    {
                                        if (highestIndex != 0)
                                            added = true;

                                    }
                                }
                                // At this point the hand was: 1 - Not a FD. 2- Not a SD. 3- Not a BDFD. 4- Not a BDSD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
                                else
                                {
                                    if (info.Item5 == 0)
                                    {
                                        if (highestIndex == 0)
                                            added = true;
                                    }
                                    else
                                    {
                                        if (highestIndex != 0)
                                            added = true;
                                    }
                                }
#endregion

                                if (added)
                                {
                                    var keyWithHighestIndex = (info.Item1, info.Item2, info.Item3, info.Item4, info.Item5);

                                    if (!bluffIndexDic.ContainsKey(pocketMask))
                                    {
                                        bluffIndexDic.Add(pocketMask, new HashSet<(bool, bool, bool, bool, byte)>());
                                        bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                        dataBluff.Add(pocketMask, new Dictionary<(bool, bool, bool, bool, byte), (double, double)>(30));
                                        dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0)); // Item 1 = probability of having the hand (before we calculate). Item2 = Sum of all unified count for the range associated to the hand.                                  
                                    }
                                    else if (!bluffIndexDic[pocketMask].Contains(keyWithHighestIndex))
                                    {
                                        bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                        dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0));
                                    }
                                    if (!countBluffRange.ContainsKey(keyWithHighestIndex))
                                    {
                                        countBluffRange.Add(keyWithHighestIndex, 0);
                                        countT[handType.Bluff] += info.Item6;
                                    }

                                    countBluffRange[keyWithHighestIndex] += tupleInfos.Item2;
                                    (double, double) value = dataBluff[pocketMask][keyWithHighestIndex];
                                    (double, double) newValue = (value.Item1, value.Item2 + info.Item6);
                                    dataBluff[pocketMask][keyWithHighestIndex] = newValue;
                                }
                            }
                        }
#endregion
                    }
                    else
                    {
                        #region Made hand
                        //double handStrength = CHandModel.CalculateHandStrength(pocketMask, _boardMask);                    
                        var b = false;
                        //if (CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer].ContainsKey(_boardMask))
                           // _indexPlayer = 1;

                        double handStrength = CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item1;

                        if (handStrength >= 0.9)
                        {
                            if (CDBHelper.PAveragePlayerValueHandsFlop.ContainsKey(key))
                            {
                                var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                var info = CDBHelper.PAveragePlayerValueHandsFlop[key].Where(x => x.Item1 == indexHS);

                                if (info.Count() > 0)
                                {
                                    double unifiedCount = info.First().Item2;

                                    if (!HsDic.ContainsKey(pocketMask))
                                        HsDic.Add(pocketMask, indexHS);
                                    if (!countHS.ContainsKey(indexHS))
                                    {
                                        countHS.Add(indexHS, 0);
                                        countT[handType.Value] += unifiedCount;
                                    }

                                    countHS[indexHS] += tupleInfos.Item2;
                                    data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                }
                            }
                        }
                        else
                        {
                            sbyte nbOuts = (sbyte)Hand.OutsDiscounted(pocketMask, _boardMask, new ulong[0]);

                            if (nbOuts >= 10)
                            {
                                if (CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityFlop.ContainsKey(key))
                                {
                                    var infos = CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityFlop[key].Where(x => nbOuts >= x.Item1);

                                    if (infos.Count() > 0)
                                    {
                                        double sampleCount = infos.First().Item2;

                                        if (!countBluffEquDic.ContainsKey(nbOuts))
                                        {
                                            countBluffEquDic.Add(nbOuts, 0); // Le 0 a pas rapport
                                            countT[handType.BluffEqu] += sampleCount;
                                        }

                                        countBluffEqu += tupleInfos.Item2;
                                        data[handType.BluffEqu].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }
                            }
                            else if (handStrength >= 0.8)
                            {
                                if (CDBHelper.PAveragePlayerValueHandsFlop.ContainsKey(key))
                                {
                                    var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                    var info = CDBHelper.PAveragePlayerValueHandsFlop[key].Where(x => x.Item1 == indexHS);

                                    if (info.Count() > 0)
                                    {
                                        double unifiedCount = info.First().Item2;

                                        if (!HsDic.ContainsKey(pocketMask))
                                            HsDic.Add(pocketMask, indexHS);
                                        if (!countHS.ContainsKey(indexHS))
                                        {
                                            countHS.Add(indexHS, 0);
                                            countT[handType.Value] += unifiedCount;
                                        }

                                        countHS[indexHS] += tupleInfos.Item2;
                                        data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                    }
                                }
                            }
                            else
                            {
                                bool isFlushDraw = Hand.IsFlushDraw(pocketMask, _boardMask, 0L);
                                bool isStraightDraw = Hand.IsStraightDraw(pocketMask, _boardMask, 0L);

                                if (isFlushDraw && isStraightDraw)
                                {
                                    if (CDBHelper.PAveragePlayerMadeHandSDAndFDFlop.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandSDAndFDFlop[key];

                                        if (!bluffSDAndFD)
                                        {
                                            countT[handType.SDFD] += infos.Item1;
                                            bluffSDAndFD = true;
                                        }

                                        countSDFD += tupleInfos.Item2;
                                        data[handType.SDFD].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }
                                else if (isFlushDraw)
                                {
                                    int indexHighestFlushDraw = CBotPokerAmigo.GetIndexHighestFlushDraw(pocketMask, _boardMask);

                                    if (CDBHelper.PAveragePlayerMadeHandFDFlop.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandFDFlop[key].Where(x => x.Item1 == indexHighestFlushDraw);

                                        if (infos.Count() > 0)
                                        {
                                            double sampleCount = infos.First().Item2;

                                            if (!FDDic.ContainsKey(pocketMask))
                                                FDDic.Add(pocketMask, (byte)indexHighestFlushDraw);
                                            if (!countBluffFD.ContainsKey(indexHighestFlushDraw))
                                            {
                                                countBluffFD.Add(indexHighestFlushDraw, 0);
                                                countT[handType.FD] += sampleCount;
                                            }

                                            countBluffFD[indexHighestFlushDraw] += tupleInfos.Item2;
                                            data[handType.FD].Add(pocketMask, (tupleInfos.Item2, sampleCount));
                                        }
                                    }
                                }
                                else if (isStraightDraw)
                                {
                                    if (CDBHelper.PAveragePlayerMadeHandSDFlop.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandSDFlop[key];

                                        if (!bluffSD)
                                        {
                                            countT[handType.SD] += infos.Item1;
                                            bluffSD = true;
                                        }

                                        countSD += tupleInfos.Item2;
                                        data[handType.SD].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }

                                else
                                {
                                    if (CDBHelper.PAveragePlayerValueHandsFlop.ContainsKey(key))
                                    {
                                        var indexHS = ((double)(((int)(handStrength * 20)) * 5) / 100);
                                        var info = CDBHelper.PAveragePlayerValueHandsFlop[key].Where(x => x.Item1 == indexHS);

                                        if (info.Count() > 0)
                                        {
                                            double unifiedCount = info.First().Item2;

                                            if (!HsDic.ContainsKey(pocketMask))
                                                HsDic.Add(pocketMask, indexHS);
                                            if (!countHS.ContainsKey(indexHS))
                                            {
                                                countT[handType.Value] += unifiedCount;
                                                countHS.Add(indexHS, 0);
                                            }

                                            countHS[indexHS] += tupleInfos.Item2;
                                            data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                        }
                                    }
                                }
                            }
                        }
#endregion
                    }
                }
            }
            void LFCompileData()
            {
                #region Initialization                
                var total = (double)0;
                foreach (var item in countT)
                    total += item.Value;
                #endregion

                foreach (var tupleInfos in _lstRange)
                {
                    Dictionary<(bool, bool, bool, bool, byte), (double, double)> infosBluff = null;
                    (double, double) infosValue;
                    (double, double) infosFD;

                    if (dataBluff.TryGetValue(tupleInfos.Item1, out infosBluff))
                    {
                        double probabilityOfCard = 0;

                        foreach (var rangeKey in infosBluff.Keys)
                        {
                            double unifiedCountOfRange = infosBluff[rangeKey].Item2; // Unified count of range

                            probabilityOfCard += ((tupleInfos.Item2 / (countBluffRange[rangeKey])) * (unifiedCountOfRange / countT[handType.Bluff]) * (countT[handType.Bluff] / total));
                        }

                        lstRange.Add((tupleInfos.Item1, probabilityOfCard));
                    }
                    else if (data[handType.BluffEqu].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countBluffEqu) * (countT[handType.BluffEqu] / total)));
                    else if (data[handType.Value].TryGetValue(tupleInfos.Item1, out infosValue))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / (countHS[HsDic[tupleInfos.Item1]])) * (infosValue.Item2 / countT[handType.Value]) * (countT[handType.Value] / total)));
                    else if (data[handType.SD].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countSD) * (countT[handType.SD] / total)));
                    else if (data[handType.FD].TryGetValue(tupleInfos.Item1, out infosFD))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countBluffFD[FDDic[tupleInfos.Item1]]) * (infosFD.Item2 / countT[handType.FD]) * (countT[handType.FD] / total)));
                    else if (data[handType.SDFD].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countSDFD) * (countT[handType.SDFD] / total)));
                }
            }
            #endregion

            LFFilterData();
            LFCompileData();

            #if DEBUG
            double toto = 0;

            foreach (var tati in lstRange)
                toto += tati.Item2;

            if (((toto < 0.99) || (toto > 1.01)) && lstRange.Count != 0)
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
            #endif

            return lstRange; // Return all lists combined
        }
        public static List<(ulong, double)> GetRangeTurn(long _turnGameStateID, BoardMetaDataFlags _boardType, List<(ulong, double)> _lstRange, ulong _boardMask, int _indexPlayer)
        {
            if (_lstRange.Count == 0)
                return _lstRange;

            long numberOfSample = CDBHelper.PAllRangeSamplesTurn[(_turnGameStateID, _boardType)];

            if (numberOfSample <= 0)
                return new List<(ulong, double)>();
            //throw new InvalidOperationException("Can't determine range. The number of sample is at 0!");

            #region Initialization
            var dataBluff = new Dictionary<ulong, Dictionary<(bool, bool, byte), (double, double)>>();
            var data = new Dictionary<handType, Dictionary<ulong, (double, double)>>();
            var countT = new Dictionary<handType, double>();
            var key = (_turnGameStateID, _boardType);

            var countBluffRange = new Dictionary<(bool, bool, byte), double>(); // First item = Number of time we counted the range
            var bluffIndexDic = new Dictionary<ulong, HashSet<(bool, bool, byte)>>();

            var countBluffEquDic = new Dictionary<int, double>(); // This is used to verify a special condition only
            double countBluffEqu = 0;

            var countBluffFD = new Dictionary<int, double>();
            var FDDic = new Dictionary<ulong, byte>();

            var countHS = new Dictionary<double, double>();
            var HsDic = new Dictionary<ulong, double>();

            double countSDFD = 0;
            double countSD = 0;

            bool bluffSDAndFD = false;
            bool bluffSD = false;

            countT.Add(handType.Bluff, 0);

            countT.Add(handType.BluffEqu, 0);
            data.Add(handType.BluffEqu, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.Value, 0);
            data.Add(handType.Value, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.SD, 0);
            data.Add(handType.SD, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.FD, 0);
            data.Add(handType.FD, new Dictionary<ulong, (double, double)>());

            countT.Add(handType.SDFD, 0);
            data.Add(handType.SDFD, new Dictionary<ulong, (double, double)>());

            List<(ulong, double)> lstRange = new List<(ulong, double)>(_lstRange.Count);
            #endregion

            #region Local methods
            void LFFilterData()
            {
                foreach (var tupleInfos in _lstRange)
                {
                    var pocketMask = tupleInfos.Item1;
                    var b = false;
                    if (CDBHelperHandInfos.PLstAllBoardsInfos[0].ContainsKey(_boardMask))
                    {
                        b = CDBHelperHandInfos.PLstAllBoardsInfos[0][_boardMask][pocketMask].Item4 == Hand.HandTypes.HighCard;
                        _indexPlayer = 0;
                    }
                    else
                    {
                        b = CDBHelperHandInfos.PLstAllBoardsInfos[1][_boardMask][pocketMask].Item4 == Hand.HandTypes.HighCard;
                        _indexPlayer = 1;
                    }

                    if (b)
                    {
                        #region Bluff hand
                        if (CDBHelper.PAveragePlayerBluffsTurn.ContainsKey(key))
                        {
                            var lstBluffs = CDBHelper.PAveragePlayerBluffsTurn[key];

                            bool isSD = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item3 & CHandModel.IS_STRAIGHT_DRAW_MASK) != 0);
                            bool isFD = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item3 & CHandModel.IS_FLUSH_DRAW_MASK) != 0);
                            byte highestIndex = (byte)(CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item3 >> 4);

                            foreach (var info in lstBluffs.Where(x => x.Item1 == isSD && x.Item2 == isFD))
                            {
                                bool added = false;

                                #region If the current hand is a bluff valid, we add it to the dictionary associated
                                if (isFD)
                                {
                                    // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                                    if (isSD)
                                    {
                                        added = true;
                                    }
                                    // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand
                                    else
                                    {
                                        if (highestIndex == info.Item5)
                                        {
                                            added = true;
                                        }
                                    }
                                }
                                // His hand was a straight draw, but not a flush draw
                                else if (isSD)
                                {
                                    // In this case, villain probably do the action with all of his straight draws
                                    if (info.Item5 == 0)
                                    {
                                        if (highestIndex == 0)
                                        {
                                            added = true;
                                        }
                                    }
                                    else
                                    {
                                        if (highestIndex != 0) // Accept all hands that are > index 0
                                        {
                                            added = true;
                                        }
                                    }
                                }
                                // At this point the hand was: 1 - Not a FD. 2- Not a SD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
                                else
                                {
                                    if (info.Item5 == 0)
                                    {
                                        if (highestIndex == 0)
                                            added = true;
                                    }
                                    else
                                    {
                                        if (highestIndex != 0)
                                            added = true;
                                    }
                                }
                                #endregion

                                if (added)
                                {
                                    var keyWithHighestIndex = (info.Item1, info.Item2, info.Item3);

                                    if (!bluffIndexDic.ContainsKey(pocketMask))
                                    {
                                        bluffIndexDic.Add(pocketMask, new HashSet<(bool, bool, byte)>());
                                        bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                        dataBluff.Add(pocketMask, new Dictionary<(bool, bool, byte), (double, double)>(30));
                                        dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0)); // Item 1 = probability of having the hand (before we calculate). Item2 = Sum of all unified count for the range associated to the hand.                                  
                                    }
                                    else if (!bluffIndexDic[pocketMask].Contains(keyWithHighestIndex))
                                    {
                                        bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                        dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0));
                                    }
                                    if (!countBluffRange.ContainsKey(keyWithHighestIndex))
                                    {
                                        countBluffRange.Add(keyWithHighestIndex, 0);
                                        countT[handType.Bluff] += info.Item4;
                                    }

                                    countBluffRange[keyWithHighestIndex] += tupleInfos.Item2;
                                    (double, double) value = dataBluff[pocketMask][keyWithHighestIndex];
                                    (double, double) newValue = (value.Item1, value.Item2 + info.Item4);
                                    dataBluff[pocketMask][keyWithHighestIndex] = newValue;
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Made hand
                        double handStrength = CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item1;

                        if (handStrength >= 0.9)
                        {
                            if (CDBHelper.PAveragePlayerValueHandsTurn.ContainsKey(key))
                            {
                                var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                var info = CDBHelper.PAveragePlayerValueHandsTurn[key].Where(x => x.Item1 == indexHS);

                                if (info.Count() > 0)
                                {
                                    double unifiedCount = info.First().Item2;

                                    if (!HsDic.ContainsKey(pocketMask))
                                        HsDic.Add(pocketMask, indexHS);
                                    if (!countHS.ContainsKey(indexHS))
                                    {
                                        countHS.Add(indexHS, 0);
                                        countT[handType.Value] += unifiedCount;
                                    }

                                    countHS[indexHS] += tupleInfos.Item2;
                                    data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                }
                            }
                        }
                        else
                        {
                            sbyte nbOuts = CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item2;

                            if (nbOuts >= 10)
                            {
                                if (CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityTurn.ContainsKey(key))
                                {
                                    var infos = CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityTurn[key].Where(x => nbOuts >= x.Item1);

                                    if (infos.Count() > 0)
                                    {
                                        double sampleCount = infos.First().Item2;

                                        if (!countBluffEquDic.ContainsKey(nbOuts))
                                        {
                                            countBluffEquDic.Add(nbOuts, 0); // Le 0 a pas rapport
                                            countT[handType.BluffEqu] += sampleCount;
                                        }

                                        countBluffEqu += tupleInfos.Item2;
                                        data[handType.BluffEqu].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }
                            }
                            else if (handStrength >= 0.8)
                            {
                                if (CDBHelper.PAveragePlayerValueHandsTurn.ContainsKey(key))
                                {
                                    var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                    var info = CDBHelper.PAveragePlayerValueHandsTurn[key].Where(x => x.Item1 == indexHS);

                                    if (info.Count() > 0)
                                    {
                                        double unifiedCount = info.First().Item2;

                                        if (!HsDic.ContainsKey(pocketMask))
                                            HsDic.Add(pocketMask, indexHS);
                                        if (!countHS.ContainsKey(indexHS))
                                        {
                                            countHS.Add(indexHS, 0);
                                            countT[handType.Value] += unifiedCount;
                                        }

                                        countHS[indexHS] += tupleInfos.Item2;
                                        data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                    }
                                }
                            }
                            else
                            {
                                bool isFlushDraw = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item3 & CHandModel.IS_FLUSH_DRAW_MASK) != 0);
                                bool isStraightDraw = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_boardMask][pocketMask].Item3 & CHandModel.IS_STRAIGHT_DRAW_MASK) != 0);

                                if (isFlushDraw && isStraightDraw)
                                {
                                    if (CDBHelper.PAveragePlayerMadeHandSDAndFDTurn.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandSDAndFDTurn[key];

                                        if (!bluffSDAndFD)
                                        {
                                            countT[handType.SDFD] += infos.Item1;
                                            bluffSDAndFD = true;
                                        }

                                        countSDFD += tupleInfos.Item2;
                                        data[handType.SDFD].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }

                                else if (isFlushDraw)
                                {
                                    int indexHighestFlushDraw = CBotPokerAmigo.GetIndexHighestFlushDraw(pocketMask, _boardMask);

                                    if (CDBHelper.PAveragePlayerMadeHandFDTurn.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandFDTurn[key].Where(x => x.Item1 == indexHighestFlushDraw);

                                        if (infos.Count() > 0)
                                        {
                                            double sampleCount = infos.First().Item2;

                                            if (!FDDic.ContainsKey(pocketMask))
                                                FDDic.Add(pocketMask, (byte)indexHighestFlushDraw);
                                            if (!countBluffFD.ContainsKey(indexHighestFlushDraw))
                                            {
                                                countBluffFD.Add(indexHighestFlushDraw, 0);
                                                countT[handType.FD] += sampleCount;
                                            }

                                            countBluffFD[indexHighestFlushDraw] += tupleInfos.Item2;
                                            data[handType.FD].Add(pocketMask, (tupleInfos.Item2, sampleCount));
                                        }
                                    }
                                }
                                else if (isStraightDraw)
                                {
                                    if (CDBHelper.PAveragePlayerMadeHandSDTurn.ContainsKey(key))
                                    {
                                        var infos = CDBHelper.PAveragePlayerMadeHandSDTurn[key];

                                        if (!bluffSD)
                                        {
                                            countT[handType.SD] += infos.Item1;
                                            bluffSD = true;
                                        }

                                        countSD += tupleInfos.Item2;
                                        data[handType.SD].Add(pocketMask, (tupleInfos.Item2, 1));
                                    }
                                }

                                else
                                {
                                    if (CDBHelper.PAveragePlayerValueHandsTurn.ContainsKey(key))
                                    {
                                        var indexHS = ((double)(((int)(handStrength * 20)) * 5) / 100);
                                        var info = CDBHelper.PAveragePlayerValueHandsTurn[key].Where(x => x.Item1 == indexHS);

                                        if (info.Count() > 0)
                                        {
                                            double unifiedCount = info.First().Item2;

                                            if (!HsDic.ContainsKey(pocketMask))
                                                HsDic.Add(pocketMask, indexHS);
                                            if (!countHS.ContainsKey(indexHS))
                                            {
                                                countT[handType.Value] += unifiedCount;
                                                countHS.Add(indexHS, 0);
                                            }

                                            countHS[indexHS] += tupleInfos.Item2;
                                            data[handType.Value].Add(pocketMask, (tupleInfos.Item2, unifiedCount));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            void LFCompileData()
            {
                #region Initialization                
                var total = (double)0;
                foreach (var item in countT)
                    total += item.Value;
                #endregion

                foreach(var tupleInfos in _lstRange)
                {
                    Dictionary<(bool, bool, byte), (double, double)> infosBluff = null;
                    (double, double) infosValue;
                    (double, double) infosFD;

                    if (dataBluff.TryGetValue(tupleInfos.Item1, out infosBluff))
                    {
                        double probabilityOfCard = 0;

                        foreach (var rangeKey in infosBluff.Keys)
                        {
                            double unifiedCountOfRange = infosBluff[rangeKey].Item2; // Unified count of range

                            probabilityOfCard += ((tupleInfos.Item2 / (countBluffRange[rangeKey])) * (unifiedCountOfRange / countT[handType.Bluff]) * (countT[handType.Bluff] / total));
                        }

                        lstRange.Add((tupleInfos.Item1, probabilityOfCard));
                    }
                    else if (data[handType.BluffEqu].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countBluffEqu) * (countT[handType.BluffEqu] / total)));
                    else if (data[handType.Value].TryGetValue(tupleInfos.Item1, out infosValue))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / (countHS[HsDic[tupleInfos.Item1]])) * (infosValue.Item2 / countT[handType.Value]) * (countT[handType.Value] / total)));
                    else if (data[handType.SD].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countSD) * (countT[handType.SD] / total)));
                    else if (data[handType.FD].TryGetValue(tupleInfos.Item1, out infosFD))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countBluffFD[FDDic[tupleInfos.Item1]]) * (infosFD.Item2 / countT[handType.FD]) * (countT[handType.FD] / total)));
                    else if (data[handType.SDFD].ContainsKey(tupleInfos.Item1))
                        lstRange.Add((tupleInfos.Item1, (tupleInfos.Item2 / countSDFD) * (countT[handType.SDFD] / total)));
                };
            }
            #endregion

            LFFilterData();
            LFCompileData();

#if DEBUG
            double toto = 0;

            foreach (var tati in lstRange)
                toto += tati.Item2;

            if (((toto < 0.99) || (toto > 1.01)) && lstRange.Count != 0)
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
#endif

            return lstRange; // Return all lists combined
        }
        private static System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        static int GetRangeRiverTimesCalled = 0;
        public static List<(ulong, double)> GetRangeRiver(long _riverGameStateID, BoardMetaDataFlags _boardType, List<(ulong, double)> _lstRange, ulong _turnBoardMask, ulong _riverBoardMask, int _indexPlayer)
        {
            if (_lstRange.Count == 0)
                return _lstRange;

            long numberOfSample = 0;

            numberOfSample = CDBHelper.PAllRangeSamplesRiver[(_riverGameStateID, _boardType)];

            if (numberOfSample <= 0)
                return new List<(ulong, double)>();
            //throw new InvalidOperationException("Can't determine range. The number of sample is at 0!");


            if (GetRangeRiverTimesCalled++ % 10000 == 0)
            {
                Console.WriteLine($"GetRangeRiver called{GetRangeRiverTimesCalled} times!");
                Console.WriteLine($"Total time taken {watch.ElapsedMilliseconds}ms!");
            }
            watch.Start();
            #region Initialization
            List<(ulong, double)> lstRangeToReturn = new List<(ulong, double)>();

            var dataBluff = new Dictionary<ulong, Dictionary<(bool, bool, byte), (double, double)>>();
            var dataBluffEqu = new Dictionary<ulong, (double, double)>();
            var dataValue = new Dictionary<ulong, (double, double)>();
            //var dataBlockers = new Dictionary<ulong, (double, double)>();

            double countTBluff = 0;
            double countTBluffEqu = 0;
            double countTValue = 0;

            var key = (_riverGameStateID, _boardType);

            var countBluffRange = new Dictionary<(bool, bool, byte), double>(); // First item = Number of time we counted the range
            var bluffIndexDic = new Dictionary<ulong, HashSet<(bool, bool, byte)>>();

            var countBluffEquDic = new Dictionary<int, long>(); // This is used to verify a special condition only
            double countBluffEqu = 0;

            var countHS = new Dictionary<double, double>();
            var HsDic = new Dictionary<ulong, double>();     
            #endregion

            #region Local methods
            void LFFilterData()
            {
                foreach(var tupleInfos in _lstRange)
                {
                    var pocketMask = tupleInfos.Item1;
                    if (CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_riverBoardMask][pocketMask].Item4 == Hand.HandTypes.HighCard)
                    {
                        #region Bluff hand

                        if (CDBHelper.PAveragePlayerBluffsRiver.ContainsKey(key))
                        {
                            // Insert the Flop state to the "to-do" list      
                            bool isSD = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_turnBoardMask][pocketMask].Item3 & CHandModel.IS_STRAIGHT_DRAW_MASK) != 0);
                            bool isFD = ((CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_turnBoardMask][pocketMask].Item3 & CHandModel.IS_FLUSH_DRAW_MASK) != 0);
                            byte highestIndex = (byte)(CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_riverBoardMask][pocketMask].Item3 >> 4);
                            List<(byte, double, long)> infos = null;

                            if (CDBHelper.PAveragePlayerBluffsRiver[key].TryGetValue((isSD, isFD), out infos))
                            {
                                foreach (var info in infos)
                                {
                                    bool added = false;

                                    #region If the current hand is a bluff valid, we add it to the dictionary associated
                                    if (isFD)
                                    {
                                        // Could only be doing the action with straight draws + FD, so we only include hands that are str8 draws + FD in this case
                                        if (isSD)
                                        {
                                            added = true;
                                        }
                                        // In this case, villain probably do the action with all flush draws that are equivalent to the highest card hand
                                        else
                                        {
                                            if (highestIndex == info.Item3)
                                            {
                                                added = true;
                                            }
                                        }
                                    }
                                    // His hand was a straight draw, but not a flush draw
                                    else if (isSD)
                                    {
                                        // In this case, villain probably do the action with all of his straight draws
                                        if (info.Item3 == 0)
                                        {
                                            if (highestIndex == 0)
                                            {
                                                added = true;
                                            }
                                        }
                                        else
                                        {
                                            if (highestIndex != 0) // Accept all hands that are > index 0
                                            {
                                                added = true;
                                            }
                                        }
                                    }
                                    // At this point the hand was: 1 - Not a FD. 2- Not a SD. This means that he did the current action with almost no equity. (Like having 7 high on 2c2h2d board). 
                                    else
                                    {
                                        if (info.Item3 == 0)
                                        {
                                            if (highestIndex == 0)
                                                added = true;
                                        }
                                        else
                                        {
                                            if (highestIndex != 0)
                                                added = true;
                                        }
                                    }
                                    #endregion

                                    if (added)
                                    {
                                        var keyWithHighestIndex = (isSD, isFD, info.Item1);

                                        if (!bluffIndexDic.ContainsKey(pocketMask))
                                        {
                                            bluffIndexDic.Add(pocketMask, new HashSet<(bool, bool, byte)>());
                                            bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                            dataBluff.Add(pocketMask, new Dictionary<(bool, bool, byte), (double, double)>(30));
                                            dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0)); // Item 1 = probability of having the hand (before we calculate). Item2 = Sum of all unified count for the range associated to the hand.                                  
                                        }
                                        else if (!bluffIndexDic[pocketMask].Contains(keyWithHighestIndex))
                                        {
                                            bluffIndexDic[pocketMask].Add(keyWithHighestIndex);
                                            dataBluff[pocketMask].Add(keyWithHighestIndex, (tupleInfos.Item2, 0));
                                        }
                                        if (!countBluffRange.ContainsKey(keyWithHighestIndex))
                                        {
                                            countBluffRange.Add(keyWithHighestIndex, 0);
                                            countTBluff += info.Item2;
                                        }

                                        countBluffRange[keyWithHighestIndex] += tupleInfos.Item2;
                                        (double, double) value = dataBluff[pocketMask][keyWithHighestIndex];
                                        (double, double) newValue = (value.Item1, value.Item2 + info.Item2);
                                        dataBluff[pocketMask][keyWithHighestIndex] = newValue;                                        
                                    }
                                }
                            }

                        }
                        #endregion
                    }
                    else
                    {
                        #region Made hand
                        double handStrength = CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_riverBoardMask][pocketMask].Item1;

                        if (handStrength >= 0.9)
                        {
                            if (CDBHelper.PAveragePlayerValueHandsRiver.ContainsKey(key))
                            {
                                var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                (double, long) info;

                                if (CDBHelper.PAveragePlayerValueHandsRiver[key].TryGetValue(indexHS, out info))
                                {
                                    double unifiedCount = info.Item2;

                                    if (!HsDic.ContainsKey(pocketMask))
                                        HsDic.Add(pocketMask, indexHS);
                                    if (!countHS.ContainsKey(indexHS))
                                    {
                                        countHS.Add(indexHS, 0);
                                        countTValue += unifiedCount;
                                    }

                                    countHS[indexHS] += tupleInfos.Item2;
                                    dataValue.Add(pocketMask, (tupleInfos.Item2, unifiedCount));                                    
                                }
                            }
                        }
                        else
                        {
                            sbyte nbOuts = CDBHelperHandInfos.PLstAllBoardsInfos[_indexPlayer][_turnBoardMask][pocketMask].Item2;

                            if (nbOuts >= 10)
                            {
                                if (CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityRiver.ContainsKey(key))
                                {
                                    var infos = CDBHelper.PAveragePlayerBluffsWithAlotsOfEquityRiver[key].Where(x => nbOuts >= x.Item1);

                                    if (infos.Count() > 0)
                                    {
                                        double sampleCount = infos.First().Item2;

                                        if (!countBluffEquDic.ContainsKey(nbOuts))
                                        {
                                            countBluffEquDic.Add(nbOuts, 0); // Le 0 a pas rapport
                                            countTBluffEqu += sampleCount;
                                        }

                                        countBluffEqu += tupleInfos.Item2;
                                        dataBluffEqu.Add(pocketMask, (tupleInfos.Item2, 1));                                        
                                    }
                                }
                            }
                            else if (handStrength >= 0.8)
                            {
                                if (CDBHelper.PAveragePlayerValueHandsRiver.ContainsKey(key))
                                {
                                    var indexHS = ((double)(((int)(handStrength * 50)) * 2) / 100);
                                    (double, long) info;

                                    if (CDBHelper.PAveragePlayerValueHandsRiver[key].TryGetValue(indexHS, out info))
                                    {
                                        double unifiedCount = info.Item2;

                                        if (!HsDic.ContainsKey(pocketMask))
                                            HsDic.Add(pocketMask, indexHS);
                                        if (!countHS.ContainsKey(indexHS))
                                        {
                                            countHS.Add(indexHS, 0);
                                            countTValue += unifiedCount;
                                        }

                                        countHS[indexHS] += tupleInfos.Item2;
                                        dataValue.Add(pocketMask, (tupleInfos.Item2, unifiedCount));                                        
                                    }
                                }
                            }
                            else
                            {
                                if (CDBHelper.PAveragePlayerValueHandsRiver.ContainsKey(key))
                                {
                                    var indexHS = ((double)(((int)(handStrength * 20)) * 5) / 100);
                                    (double, long) info;

                                    if (CDBHelper.PAveragePlayerValueHandsRiver[key].TryGetValue(indexHS, out info))
                                    {
                                        double unifiedCount = info.Item2;

                                        if (!HsDic.ContainsKey(pocketMask))
                                            HsDic.Add(pocketMask, indexHS);
                                        if (!countHS.ContainsKey(indexHS))
                                        {
                                            countTValue += unifiedCount;
                                            countHS.Add(indexHS, 0);
                                        }

                                        countHS[indexHS] += tupleInfos.Item2;
                                        dataValue.Add(pocketMask, (tupleInfos.Item2, unifiedCount));                                        
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            void LFCompileData()
            {
                double total = (countTBluff + countTBluffEqu + countTValue);

                foreach(var tupleInfos in _lstRange)
                {
                    Dictionary<(bool, bool, byte), (double, double)> infosBluff = null;
                    (double, double) infosValue;

                    if (dataBluff.TryGetValue(tupleInfos.Item1, out infosBluff))
                    {
                        double probabilityOfCard = 0;

                        foreach (var rangeKey in infosBluff.Keys)
                        {
                            double unifiedCountOfRange = infosBluff[rangeKey].Item2; // Unified count of range

                            probabilityOfCard += ((tupleInfos.Item2 / countBluffRange[rangeKey]) * (unifiedCountOfRange / countTBluff) * (countTBluff / total));
                        }

                        lstRangeToReturn.Add((tupleInfos.Item1, probabilityOfCard));
                        
                    }
                    else if (dataBluffEqu.ContainsKey(tupleInfos.Item1))                    
                        lstRangeToReturn.Add((tupleInfos.Item1, (tupleInfos.Item2 / countBluffEqu) * (countTBluffEqu / total)));                                            
                    else if (dataValue.TryGetValue(tupleInfos.Item1, out infosValue))                    
                        lstRangeToReturn.Add((tupleInfos.Item1, (tupleInfos.Item2 / (countHS[HsDic[tupleInfos.Item1]])) * (infosValue.Item2 / countTValue) * (countTValue / total)));                                            
                    // Missing the blockers here
                };
            }
            #endregion
            LFFilterData();
            LFCompileData();

            #if DEBUG
            double toto = 0;
            foreach (var tati in lstRangeToReturn)
                toto += tati.Item2;

            if (((toto < 0.99) || (toto > 1.01)) && lstRangeToReturn.Count != 0)
                throw new Exception("Probabilities is not close to 1. Are the probabilities sum correctly?");
#endif
            watch.Stop();
            return lstRangeToReturn; // Return all lists combined
        }

#region Utility functions
        static public byte CardIndex(ulong _pocketMask, ulong _boardMask)
        {
            if (Hand.BitCount(_pocketMask) != 1 && Hand.BitCount(_pocketMask) != 2)
                throw new Exception("Expected one card mask");
            if (Hand.BitCount(_boardMask) < 3)
                throw new Exception("Expected at least 3 cards board");


            var cardRank = 0;
            foreach (var card in Hand.Cards(_pocketMask))
            {
                cardRank = Math.Max(cardRank, Hand.CardRank(Hand.ParseCard(card)));
            }
            var cardIndex = (byte)(Hand.RankAce - cardRank);

            var visitedCard = new Dictionary<int, int>();

            foreach (var card in Hand.Cards(_boardMask))
            {
                var cr = Hand.CardRank(Hand.ParseCard(card));
                if (!visitedCard.ContainsKey(cr))
                {
                    if (cr > cardRank)
                        cardIndex--;
                    visitedCard.Add(cr, 0);
                }
            }

            return cardIndex;
        }
        static public int GetIndexHighestFlushDraw(ulong _pocket, ulong _boardMask, ulong _deadCards = 0)
        {
            var colorArray = new int[]{ 0, 0, 0, 0 };
            var colorType = 0;

            foreach (var cardString in Hand.Cards(_boardMask | _pocket))
            {
                var cardNumber = Hand.ParseCard(cardString);
                var cardSuit = Hand.CardSuit(cardNumber);
                if (++colorArray[cardSuit] > 3) colorType = cardSuit;
            }

            if (colorArray[colorType] < 4)
                throw new Exception("Not a flush draw");

            var cardIdx = new List<byte>() { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            var visitedCardMask = UInt16.MinValue;

            foreach (var cardString in Hand.Cards(_boardMask))
            {
                var cardNumber = Hand.ParseCard(cardString);
                var cardSuit = Hand.CardSuit(cardNumber);
                var cardRank = Hand.CardRank(cardNumber);
                if ((cardSuit == colorType) && ((visitedCardMask & (1 << cardRank)) == 0))
                {
                    for (int i = 0; i < cardRank; --cardIdx[i++]) ;
                    visitedCardMask |= (ushort)(1 << cardRank);
                }
            }

            var ret = cardIdx[0];
            var pocketCardValueList = Hand.Cards(_pocket).Select(x => Hand.ParseCard(x)).ToArray();
            if (Hand.CardSuit(pocketCardValueList[0]) == colorType) ret = cardIdx[Hand.CardRank(pocketCardValueList[0])];
            if (Hand.CardSuit(pocketCardValueList[1]) == colorType) ret = Math.Min(ret, cardIdx[Hand.CardRank(pocketCardValueList[1])]);

            return ret;
        }
        static public int GetIndexHighestFlushDrawOffSuitedPockets(ulong _pocket, ulong _boardMask, ulong _deadCards = 0)
        {
            var pocketCardSuitArray = Hand.Cards(_pocket).Select(x => Hand.CardSuit(Hand.ParseCard(x))).ToArray();
            if (pocketCardSuitArray[0] == pocketCardSuitArray[1])
                throw new Exception("Pocket cards suited. I crash so you dont do retarded things :). Your welcome");
            return GetIndexHighestFlushDraw(_pocket, _boardMask, _deadCards);
        }
        static public List<ulong> GetNutsHands(Hand.HandTypes _handTypeMin, List<Tuple<ulong, double>> _hands, ulong _boardMask, ulong _deadCards = 0)
        {
            var ret = new List<ulong>();
            var ind = 0;
            var handType = Hand.HandTypes.HighCard;
            do
            {
                ret.Add(_hands[ind].Item1);
                handType = Hand.EvaluateType(_hands[++ind].Item1 | _boardMask);
            } while (handType >= _handTypeMin);
            return ret;
        }

        static public List<Tuple<ulong, double>> GetAllBlockers(List<Tuple<ulong, double>> _pockets, ulong _boardMask, ulong _deadCards = 0)
        {
            var boardMetaData = CBoardModel.CalculateMetaData(_boardMask);
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.All ^ CBoardModel.BoardMetaDataFlags.FlushPossible ^ CBoardModel.BoardMetaDataFlags.StraightPossible ^ CBoardModel.BoardMetaDataFlags.Paired)) == 0)
                throw new Exception("Board without any possible blockers");
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.FullHouse | CBoardModel.BoardMetaDataFlags.Quads)) != 0)
                throw new Exception("Board with better than fullhouse on it");
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.StraightComplete | CBoardModel.BoardMetaDataFlags.FlushComplete)) == (CBoardModel.BoardMetaDataFlags.StraightComplete | CBoardModel.BoardMetaDataFlags.FlushComplete))
                throw new Exception("Board with better than fullhouse on it");

            var minHandType = (Hand.HandTypes)Math.Min((int)Hand.HandTypes.FullHouse, (int)Hand.EvaluateType(_pockets[0].Item1 | _boardMask));
            var nutsHandsList = GetNutsHands(minHandType, _pockets, _boardMask, _deadCards);

            switch (minHandType)
            {
                case Hand.HandTypes.Straight:
                    var valueNut = Hand.Evaluate(nutsHandsList[0] | _boardMask);
                    nutsHandsList.RemoveAll(x =>
                    {
                        return Hand.Evaluate(x | _boardMask) != valueNut;
                    });
                    break;
                case Hand.HandTypes.Flush:
                    var flushIdx = 0;
                    var cardIdx = new List<byte>() { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
                    var visitedCard = new Dictionary<int, int>();
                    foreach (var card in Hand.Cards(_boardMask))
                    {
                        var cr = Hand.CardRank(Hand.ParseCard(card));
                        if (!visitedCard.ContainsKey(cr))
                        {
                            for (int i = 0; i < cr; i++)
                            {
                                cardIdx[i]--;
                            }
                            visitedCard.Add(cr, 0);
                        }
                    }
                    nutsHandsList.RemoveAll(x =>
                    {
                        var cardRank = 0;
                        foreach (var card in Hand.Cards(x))
                        {
                            cardRank = Math.Max(cardRank, Hand.CardRank(Hand.ParseCard(card)));
                        }
                        return cardIdx[cardRank] > flushIdx;
                    });
                    break;
                default:
                    break;
            }
            var cardMask = ulong.MinValue;
            foreach (var pocket in nutsHandsList)
            {
                cardMask |= pocket;
            }
            var ghj = nutsHandsList.ConvertAll(x => Hand.Cards(x).Aggregate((y, z) => y + z));
            var ret = new List<Tuple<ulong, double>>();
            foreach (var cardOne in Hand.Hands(0, ~cardMask, 1))
            {
                foreach (var cardTwo in Hand.Hands(0, _boardMask | _deadCards | cardOne, 1))
                {
                    var pocket = cardOne | cardTwo;
                    if (ret.Any(x => x.Item1 == pocket)) continue;
                    if (nutsHandsList.Any(x => x == pocket)) continue;
                    if (Hand.EvaluateType(pocket | _boardMask) >= minHandType) continue;

                    var overlapCount = nutsHandsList.LongCount(x => (x & pocket) != 0);
                    ret.Add(new Tuple<ulong, double>(pocket, (1.0 * overlapCount / nutsHandsList.Count)));
                }
            }
            ret.Sort((x, y) =>
            {
                if (x.Item2 < y.Item2) return 1;
                if (x.Item2 > y.Item2) return -1;
                if (Hand.Evaluate(x.Item1) < Hand.Evaluate(y.Item1)) return -1;
                if (Hand.Evaluate(x.Item1) > Hand.Evaluate(y.Item1)) return 1;
                return 0;
            });
            return ret;
        }
        static public List<Tuple<ulong, double>> GetAllBlockers(ulong _boardMask, ulong _deadCards = 0)
        {
            var handList = Hand.HandStrengthList(_boardMask, _deadCards);
            return GetAllBlockers(handList, _boardMask, _deadCards);
        }
        static public List<Tuple<ulong, double>> GetBlockerRangeFromPocket(List<Tuple<ulong, double>> _hands, ulong _pocketMask, ulong _boardMask, ulong _deadCards = 0)
        {
            var boardMetaData = CBoardModel.CalculateMetaData(_boardMask);
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.All ^ CBoardModel.BoardMetaDataFlags.FlushDrawPossible ^ CBoardModel.BoardMetaDataFlags.StraightDrawPossible)) == 0)
                throw new Exception("Board without any possible blockers");
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.FullHouse | CBoardModel.BoardMetaDataFlags.Quads)) != 0)
                throw new Exception("Board with better than fullhouse on it");
            if ((boardMetaData & (CBoardModel.BoardMetaDataFlags.StraightComplete | CBoardModel.BoardMetaDataFlags.FlushComplete)) == (CBoardModel.BoardMetaDataFlags.StraightComplete | CBoardModel.BoardMetaDataFlags.FlushComplete))
                throw new Exception("Board with better than fullhouse on it");

            var minHandType = (Hand.HandTypes)Math.Min((int)Hand.HandTypes.FullHouse, (int)Hand.EvaluateType(_hands[0].Item1 | _boardMask));
            var nutsHandsList = GetNutsHands(minHandType, _hands, _boardMask, _deadCards);

            switch (minHandType)
            {
                case Hand.HandTypes.Straight:
                    var valueNut = Hand.Evaluate(nutsHandsList[0] | _boardMask);
                    nutsHandsList.RemoveAll(x =>
                    {
                        return Hand.Evaluate(x | _boardMask) != valueNut;
                    });
                    break;
                case Hand.HandTypes.Flush:
                    var flushIdx = 0;
                    var cardIdx = new List<byte>() { 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
                    var visitedCard = new Dictionary<int, int>();
                    foreach (var card in Hand.Cards(_boardMask))
                    {
                        var cr = Hand.CardRank(Hand.ParseCard(card));
                        if (!visitedCard.ContainsKey(cr))
                        {
                            for (int i = 0; i < cr; i++)
                            {
                                cardIdx[i]--;
                            }
                            visitedCard.Add(cr, 0);
                        }
                    }
                    nutsHandsList.RemoveAll(x =>
                    {
                        var cardRank = 0;
                        foreach (var card in Hand.Cards(x))
                        {
                            cardRank = Math.Max(cardRank, Hand.CardRank(Hand.ParseCard(card)));
                        }
                        return cardIdx[cardRank] > flushIdx;
                    });
                    break;
                default:
                    break;
            }
            var cardMask = ulong.MinValue;
            foreach (var pocket in nutsHandsList)
            {
                cardMask |= pocket;
            }
            var overlapCount = nutsHandsList.LongCount(x => (x & _pocketMask) != 0);

            var ret = new List<Tuple<ulong, double>>();
            foreach (var cardOne in Hand.Hands(0, ~cardMask, 1))
            {
                foreach (var cardTwo in Hand.Hands(0, _boardMask | _deadCards | cardOne, 1))
                {
                    var pocket = cardOne | cardTwo;
                    if (nutsHandsList.Any(x => x == pocket)) continue;
                    //if (Hand.EvaluateType(pocket | _boardMask) >= minHandType) continue;
                    if (ret.Any(x => x.Item1 == pocket)) continue;

                    var oC = nutsHandsList.LongCount(x => (x & pocket) != 0);
                    if (oC == overlapCount)
                    {
                        if ((1.0 * overlapCount / nutsHandsList.Count) > 0.20)
                        {
                            ret.Add(new Tuple<ulong, double>(pocket, (1.0 * overlapCount / nutsHandsList.Count)));
                        }
                    }
                }
            }
            ret.Sort((x, y) =>
            {
                if (x.Item2 < y.Item2) return 1;
                if (x.Item2 > y.Item2) return -1;
                if (Hand.Evaluate(x.Item1) < Hand.Evaluate(y.Item1)) return -1;
                if (Hand.Evaluate(x.Item1) > Hand.Evaluate(y.Item1)) return 1;
                return 0;
            });
            return ret;
        }
        static public List<Tuple<ulong, double>> GetBlockerRangeFromPocket(ulong _pocketMask, ulong _boardMask, ulong _deadCards = 0)
        {
            var handList = Hand.HandStrengthList(_boardMask, _deadCards);
            return GetBlockerRangeFromPocket(handList, _pocketMask, _boardMask, _deadCards);
        }
        static public List<ulong> GetRangeFromBlockerRatioAndHandRatio(List<Tuple<ulong, double>> _hands, ulong _boardMask, ulong _deadCards, List<ulong> _rangeToSearchInto, List<ulong> _allowedRange, double _blockerRatio, double _handRatio, double _alpha = 0.01)
        {
            var blockerList = GetAllBlockers(_hands, _boardMask, _deadCards);
            blockerList.RemoveAll(x => Math.Abs(x.Item2 - _blockerRatio) > _alpha);
            var qwe = 1.0 * blockerList.Count * _handRatio;
            blockerList = blockerList.Take((int)(1.0 * blockerList.Count * _handRatio)).ToList();
            blockerList.RemoveAll(x => !_rangeToSearchInto.Any(y => x.Item1 == y));
            blockerList.RemoveAll(x => !_allowedRange.Any(y => x.Item1 == y));
            return blockerList.ConvertAll(x => x.Item1);
        }
        static public List<ulong> GetRangeFromBlockerRatioAndHandRatio(ulong _boardMask, ulong _deadCards, List<ulong> _rangeToSearchInto, List<ulong> _allowedRange, double _blockerRatio, double _handRatio, double _alpha = 0.01)
        {
            var handList = Hand.HandStrengthList(_boardMask, _deadCards);
            return GetRangeFromBlockerRatioAndHandRatio(handList, _boardMask, _deadCards, _rangeToSearchInto, _allowedRange, _blockerRatio, _handRatio, _alpha);
        }
        static public double GetHandRatioFromBlockerRange(List<Tuple<ulong, double>> _range, ulong _pocketMask)
        {
            if (!_range.Any(x => x.Item1 == _pocketMask))
                throw new Exception("Hand not in range");
            return 1.0 * (_range.FindLastIndex(x => Hand.Evaluate(x.Item1) == Hand.Evaluate(_pocketMask)) + 1) / _range.Count;
        }
        static public bool IsNutHandHigherThanTrips(ulong _boardMask)
        {
            return (CBoardModel.CalculateMetaData(_boardMask) & (CBoardModel.BoardMetaDataFlags.All ^ CBoardModel.BoardMetaDataFlags.FlushDrawPossible ^ CBoardModel.BoardMetaDataFlags.StraightDrawPossible)) != 0;
        }
#endregion
#endregion
    }
}
