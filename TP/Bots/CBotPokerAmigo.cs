using System;
using System.Collections.Generic;
using static Amigo.Models.CAction;
using static Amigo.Models.CPlayer;
using static Amigo.Core.CTableInfos;
using HoldemHand;
using Amigo.Helpers;
using System.Diagnostics;
using Amigo.Models;
using Amigo.Core;
using Amigo.Controllers;

namespace Amigo.Bots
{
    /// <summary>
    /// Equivalent of static class but is not a static class because it will be used for multi threading
    /// </summary>
    public class CBotPokerAmigo : CBotPoker
    {
        //private enum BetSizePossible { Percent33 = 33, Percent50 = 50, Percent72 = 72, Percent100 = 100, Percent133 = 133, AllIn };

        private List<CComboCard> FFVillainRange;

        public CBotPokerAmigo()
        {
            FFVillainRange = new List<CComboCard>();
        }

        public override CAction GetDecision(CTableInfosNLHE2Max _headsUpTable, CGame2MaxManualController _simulatorThatRepresentCurrentGame)
        {
            CAction finalDecision = null;

            #region Méthodes
            Action<decimal> MakeDecisionBetween2BBAnd3BBOpen = (decimal _openSize) =>
            {
                const int MARGE_ERREUR = 1;

                string preflopVillainRange = null;

                if (_openSize == 3)
                    preflopVillainRange = "22+ A2s+ K2s+ Q7s+ J7s+ T7s+ 96s+ 85s+ 74s+ 64s+ 32s+ A2o+ K8o+ Q9o+ J8o+ 98o+";
                else
                    preflopVillainRange = "22+ A2s+ K2s+ Q2s+ J2s+ T4s+ 94s+ 84s+ 74s+ 63s+ 32s+ A2o+ K6o+ Q6o+ J6o+ 67o+";

                decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false);
                decimal chipsThatWeNeedToPut = (_openSize - _headsUpTable.PHero.PLastBet);
                decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                if ((preflopEquity - MARGE_ERREUR) > potOdds)
                {
                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                    finalDecision = Call();
                }
                else
                {
                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                    finalDecision = Fold();
                }
            };
            Action<decimal> MakeDecisionBetween2BBAnd8BBOpen = (decimal _openSize) =>
            {            
                const int MARGE_ERREUR = 1;

                if (_headsUpTable.isOurHandInThisRange("JJ+ AKo AKs"))
                {
                    finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(_openSize, (decimal)4.3));
                }
                else if (_headsUpTable.isOurHandInThisRange("99 TT AQo AQs AJs A2s A3s A4s A5s JTs+ KJs"))
                {
                    Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                    int randomNumber = random.Next(0, 100);

                    // 30% of the time, we'll 3bet JTs+ KJs AJs-AQs AQo 99-TT
                    if (randomNumber >= 70)
                        finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(_openSize, (decimal)4.3));
                    else
                        finalDecision = Call();
                }
                else if (_headsUpTable.isOurHandInThisRange("45s 56s 67s 78s 89s 9Ts 75s 64s 53s A8s A7s A9s A6s KTs Q9s Q8s J9s QTs K5s Q5s KQo A5o"))
                {
                    Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                    int randomNumber = random.Next(0, 100);

                    // 10% of the time, we'll 3bet the bluff range
                    if (randomNumber >= 90)
                        finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(_openSize, (decimal)4.3));
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

                        decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false);
                        decimal chipsThatWeNeedToPut = (_openSize - _headsUpTable.PHero.PLastBet);
                        decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                        CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                        if ((preflopEquity - MARGE_ERREUR) > potOdds)
                        {
                            CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                            finalDecision = Call();
                        }
                        else
                        {
                            CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                            finalDecision = Fold();
                        }
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

                    decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(preflopVillainRange), false);
                    decimal chipsThatWeNeedToPut = (_openSize - _headsUpTable.PHero.PLastBet);
                    decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                    CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                    if ((preflopEquity - MARGE_ERREUR) > potOdds)
                    {
                        CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                        finalDecision = Call();
                    }
                    else
                    {
                        CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                        finalDecision = Fold();
                    }
                    #endregion
                }

            };
            #endregion

            try
            {
                FFTimer.Restart();
                base.InitializeNewInformations(_headsUpTable);

                // If we just switched on the flop (new street)
                if (FFStreetLastDecision != FFCurrentStreet)
                {
                    ActionsPossible lastActionFromVillain = FFTableInfos.GetLastActionFromVillain().PAction;

                    // Update probabilities of cards
                    foreach (CComboCard currentVillainCards in FFVillainRange)
                        currentVillainCards.OnStreetChanged(lastActionFromVillain);
                }

                switch (FFCurrentStreet)
                {
                    case ToursPossible.Preflop:
                        FFVillainRange.Clear();

                        switch (FFPreflopTypePot)
                        {
                            case TypesPot.Limped:
                                finalDecision = new CAction(ActionsPossible.Raise, 5);
                                break;
                            case TypesPot.OneBet:
                                #region We're from BTN. Nothing happened.

                                CLogger.AddLog(new CLog("We're from BTN"));
                                CLogger.AddLog(new CLog("Our hand is " + FFHeroCards));

                                #region If effectives stacks are <= 18BB
                                if (FFEffectiveStacksRounded <= 18)
                                {
                                    switch (FFEffectiveStacksRounded)
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
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q3o+ J2s+ J7o+ T4s+ T7o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 9:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q5o+ J2s+ J8o+ T5s+ T8o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 10:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q7o+ J3s+ J8o+ T5s+ T8o+ 95s+ 97o+ 85s+ 87o 74s+ 64s+ 53s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 11:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2o+ K2s+ Q2s+ Q8o+ J4s+ J8o+ T5s+ T8o+ 95s+ 98o 85s+ 87o 74s+ 64s+ 53s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 12:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K3o+ Q2s+ Q8o+ J5s+ J9o+ T6s+ T8o+ 95s+ 98o 85s+ 87o 75s+ 64s+ 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 13:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K4o+ Q3s+ Q8o+ J5s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 64s+ 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 14:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K5o+ Q4s+ Q9o+ J5s+ J8o+ T6s+ T8o+ 95s+ 98o 85s+ 87o 75s+ 64s+ 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 15:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K6o+ Q4s+ Q9o+ J6s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 64s+ 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 16:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K7o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T9o 96s+ 98o 85s+ 75s+ 65s 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 17:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K8o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T8o+ 96s+ 98o 85s+ 75s+ 65s 54s"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                            break;
                                        case 18:
                                            if (_headsUpTable.isOurHandInThisRange("22+ A2o+ A2s+ K2s+ K8o+ Q5s+ Q9o+ J6s+ J9o+ T6s+ T9o 96s+ 98o 85s+ 75s+ 65s 54s"))
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

                                    if (_headsUpTable.isOurHandInThisRange(openRange))
                                    {
                                        if (FFEffectiveStacks <= 40)
                                            finalDecision = new CAction(ActionsPossible.Raise, 2);
                                        else
                                            finalDecision = new CAction(ActionsPossible.Raise, 3);
                                    }
                                    else
                                        finalDecision = Fold();
                                }
                                #endregion
                                break;
                            case TypesPot.TwoBet:
                                #region We're on the BB. BTN opened.                            
                                decimal openSize = FFTableInfos.PVillain.PLastBet;

                                if (FFEffectiveStacks <= 0)
                                    throw new Exception("FFEffectiveStacks are smaller than 0BB. This is not normal! The bot cannot take a decision.");
                                else if (openSize < 2)
                                    throw new Exception("Cannot detect the open of the preflop raiser! Open size was less than 2BB! The bot cannot take a decision.");

                                if (FFEffectiveStacks <= 70)
                                {
                                    
                                    if (FFEffectiveStacks <= 20)
                                    {
                                        if (openSize <= 3)
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("66+ ATo+ KJs+ A8s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                MakeDecisionBetween2BBAnd3BBOpen(openSize);
                                        }
                                        else
                                        {
                                            bool raiseAllIn = false;

                                            if (FFEffectiveStacks <= 8)                                            
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ K6o+ K2s+ A2o+ A2s+ QJo Q6s+"));
                                            else if (FFEffectiveStacks == 9)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ K8o+ K2s+ A2o+ A2s+ QJo Q8s+"));
                                            else if (FFEffectiveStacks == 10)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ KTo+ K3s+ A2o+ A2s+ Q9s+"));
                                            else if (FFEffectiveStacks == 11)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ KTo+ K5s+ A2o+ A2s+ QTs+"));
                                            else if (FFEffectiveStacks == 12)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ KJo+ K7s+ A2o+ A2s+ QJs"));
                                            else if (FFEffectiveStacks == 13)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("22+ KJo+ K8s+ A2o+ A2s+"));
                                            else if (FFEffectiveStacks == 14)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("33+ KQo K9s+ A2o+ A2s+"));
                                            else if (FFEffectiveStacks == 15)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("44+ KQo KTs+ A3o+ A2s+"));
                                            else if (FFEffectiveStacks == 16)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("44+ KQo KJs+ A4o+ A2s+"));
                                            else if (FFEffectiveStacks == 17)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("44+ KJs+ A4o+ A2s+"));
                                            else if (FFEffectiveStacks <= 20)
                                                raiseAllIn = (_headsUpTable.isOurHandInThisRange("44+ KQs A5o+ A2s+"));

                                            if (raiseAllIn)
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                        }
                                    }
                                    #region Stacks are between 21BB and 70BB
                                    else if (FFEffectiveStacks <= 30)
                                    {
                                        #region Stack is <= 30BB
                                        if (openSize <= 3)
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("66+ ATo+ KJs+ A8s+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                MakeDecisionBetween2BBAnd3BBOpen(openSize);
                                        }
                                        else
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("66+ A6o+ KJs+ A4s+"))
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
                                            if (_headsUpTable.isOurHandInThisRange("KJs+ AJs+ AJo+ 99+"))
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
                                else if (FFEffectiveStacks <= 115)
                                {
                                    #region Stacks are between 71BB and 115BB
                                    #region If the open is >= 8BB
                                    if (openSize >= 8)
                                    {
                                        if (openSize <= 20)
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("KJs+ AJs+ AJo+ 99+"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                        }
                                        else
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("AJs+ AJo+ TT+"))
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

                                            if (FFEffectiveStacks >= 200)
                                                range = "AQo+ KJs+ ATs+ 88+";
                                            else
                                                range = "AQo+ AJs+ TT+";

                                            if (_headsUpTable.isOurHandInThisRange(range))
                                                finalDecision = Call();
                                            else
                                                finalDecision = Fold();
                                        }
                                        #endregion
                                        #region If the open is higher than 25BB
                                        else
                                        {
                                            string range = null;

                                            if (FFEffectiveStacks >= 200)
                                                range = "AKo+ AKs+ KK+";
                                            else
                                                range = "AKo+ AKs+ JJ+";

                                            if (_headsUpTable.isOurHandInThisRange(range))
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
                                const int MARGE_ERREUR = 1;

                                decimal heroOpenSize = _headsUpTable.PHero.PLastBet;
                                decimal villain3BetSize = _headsUpTable.PVillain.PLastBet;
                                string villain3BetRange = "TT+ AJs+ KJs+ QJs JTs T9s 98s 87s 76s 65s AQo+";

                                if (FFEffectiveStacks <= 30)
                                {
                                    #region Effective stacks are <= 30BB
                                    if (_headsUpTable.isOurHandInThisRange("99+ ATo+ KQs+ ATs+"))
                                        finalDecision = RaiseAllIn();
                                    else if (villain3BetSize < 9)
                                    {
                                        decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                        decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                        decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                        CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                        // -3 because we're short stack. We won't realize our equity turn/river because the play is mostly done by the flop.
                                        if ((preflopEquity - MARGE_ERREUR - 3) > potOdds)
                                        {
                                            CLogger.AddLog(new CLog("(Preflop realized equity - 4) is higher than pot odds, calling..."));
                                            finalDecision = Call();
                                        }
                                        else
                                        {
                                            CLogger.AddLog(new CLog("(Preflop realized equity - 4) is lower than pot odds, folding..."));
                                            finalDecision = Fold();
                                        }
                                    }
                                    else
                                        finalDecision = Fold();
                                    #endregion
                                }
                                else if (FFEffectiveStacks <= 60)
                                {
                                    #region Effective stacks are <= 60BB
                                    if (_headsUpTable.isOurHandInThisRange("JJ+ AQs+ AQo+"))
                                        finalDecision = RaiseAllIn();
                                    else if (villain3BetSize <= 13)
                                    {
                                        decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                        decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                        decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                        CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                        // -3 because we're short stack. We won't realize our equity turn/river because the play is mostly done by the flop.
                                        if ((preflopEquity - MARGE_ERREUR - 2) > potOdds)
                                        {
                                            CLogger.AddLog(new CLog("(Preflop realized equity - 3) is higher than pot odds, calling..."));
                                            finalDecision = Call();
                                        }
                                        else
                                        {
                                            CLogger.AddLog(new CLog("(Preflop realized equity - 3) is lower than pot odds, folding..."));
                                            finalDecision = Fold();
                                        }
                                    }
                                    else
                                        finalDecision = Fold();
                                    #endregion
                                }
                                else if (FFEffectiveStacks <= 120)
                                {
                                    #region Effective stacks are <= 120BB
                                    bool normal3BetSize = (villain3BetSize <= 13);

                                    if (normal3BetSize)
                                    {
                                        if (_headsUpTable.isOurHandInThisRange("JJ+ AKo AKs"))
                                        {
                                            #region 4bet range 70% of the time (value only)
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 70% of the time, we'll 4bet JJ+ AK+
                                            if (randomNumber >= 30)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, (decimal)2.25));
                                            else
                                                finalDecision = Call();
                                            #endregion
                                        }
                                        else if (_headsUpTable.isOurHandInThisRange("KQs AJs"))
                                        {
                                            #region 4bet range 20% of the time (value/bluff sometimes)
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 20% of the time, we'll 4bet KQs AJs
                                            if (randomNumber >= 80)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, (decimal)2.25));
                                            else
                                                finalDecision = Call();
                                            #endregion
                                        }
                                        else if (_headsUpTable.isOurHandInThisRange("67s 68s 97s A2s A3s A4s A5s 98s 78s 75s 56s AQo A5o KQo"))
                                        {
                                            #region 4bet range 10% of the time (bluffs)
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 10% of the time, we'll 4bet 67s 68s 97s A2s-A5s 98s 78s 75s 56s AQo A5o KQo
                                            if (randomNumber >= 90)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, (decimal)2.25));
                                            else
                                            {
                                                decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                                decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                                decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                                CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                                if ((preflopEquity - MARGE_ERREUR) > potOdds)
                                                {
                                                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                                                    finalDecision = Call();
                                                }
                                                else
                                                {
                                                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                                                    finalDecision = Fold();
                                                }
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            #region Not in the 4bet range. Whatever hand we got, we calculate odds before calling.
                                            decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                            decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                            decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                            CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                            if ((preflopEquity - MARGE_ERREUR) > potOdds)
                                            {
                                                CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                                                finalDecision = Call();
                                            }
                                            else
                                            {
                                                CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                                                finalDecision = Fold();
                                            }
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region 4bet range only in value since very high sizing
                                        if (_headsUpTable.isOurHandInThisRange("JJ+ AKo AKs"))
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
                                        if (_headsUpTable.isOurHandInThisRange("JJ+ AKo AKs"))
                                        {
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 70% of the time, we'll 4bet JJ+ AK+
                                            if (randomNumber >= 30)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, 3));
                                            else
                                                finalDecision = Call();
                                        }
                                        else if (_headsUpTable.isOurHandInThisRange("KQs AJs"))
                                        {
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 20% of the time, we'll 4bet KQs AJs
                                            if (randomNumber >= 80)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, 3));
                                            else
                                                finalDecision = Call();
                                        }
                                        else if (_headsUpTable.isOurHandInThisRange("67s 68s 97s A2s A3s A4s A5s 98s 78s 75s 56s AQo A5o KQo"))
                                        {
                                            Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                            int randomNumber = random.Next(0, 100);

                                            // 10% of the time, we'll 4bet 67s 68s 97s A2s-A5s 98s 78s 75s 56s AQo A5o KQo
                                            if (randomNumber >= 90)
                                                finalDecision = new CAction(ActionsPossible.Raise, decimal.Multiply(villain3BetSize, 3));
                                            else
                                            {
                                                decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                                decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                                decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                                CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                                if ((preflopEquity - MARGE_ERREUR) > potOdds)
                                                {
                                                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                                                    finalDecision = Call();
                                                }
                                                else
                                                {
                                                    CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                                                    finalDecision = Fold();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            decimal preflopEquity = CalculatePreflopRealizedEquity(_headsUpTable.PHeroCards.Item1, _headsUpTable.PHeroCards.Item2, CPokerRangeConverter.GetInstance().ConvertRange(villain3BetRange), false);
                                            decimal chipsThatWeNeedToPut = (villain3BetSize - heroOpenSize);
                                            decimal potOdds = Math.Round((chipsThatWeNeedToPut / (_headsUpTable.PPot + chipsThatWeNeedToPut)) * 100, 2);

                                            CLogger.AddLog(new CLog("Pot odds: " + potOdds + "%"));

                                            if ((preflopEquity - MARGE_ERREUR) > potOdds)
                                            {
                                                CLogger.AddLog(new CLog("(Preflop realized equity - 1) is higher than pot odds, calling..."));
                                                finalDecision = Call();
                                            }
                                            else
                                            {
                                                CLogger.AddLog(new CLog("(Preflop realized equity - 1) is lower than pot odds, folding..."));
                                                finalDecision = Fold();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (villain3BetSize < 30)
                                        {
                                            if (_headsUpTable.isOurHandInThisRange("AA"))
                                            {
                                                Random random = new Random(System.DateTime.UtcNow.Millisecond % 105143);
                                                int randomNumber = random.Next(0, 100);

                                                // 70% of the time, we'll 4bet jam only AA
                                                if (randomNumber >= 30)
                                                    finalDecision = RaiseAllIn();
                                                else
                                                    finalDecision = Call();
                                            }
                                            else if (_headsUpTable.isOurHandInThisRange("JJ+ AKo AKs"))
                                                finalDecision = Call();
                                            else
                                                finalDecision = Fold();
                                        }
                                        else
                                        {
                                            if (FFEffectiveStacksRounded <= 200)
                                            {
                                                if (_headsUpTable.isOurHandInThisRange("AKo AKs QQ+"))
                                                    finalDecision = RaiseAllIn();
                                                else
                                                    finalDecision = Fold();
                                            }
                                            else if (FFEffectiveStacksRounded <= 300)
                                            {
                                                if (_headsUpTable.isOurHandInThisRange("KK+"))
                                                    finalDecision = RaiseAllIn();
                                                else
                                                    finalDecision = Fold();
                                            }
                                            else if (_headsUpTable.isOurHandInThisRange("AA"))
                                                finalDecision = RaiseAllIn();
                                            else
                                                finalDecision = Fold();
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                break;
                            case TypesPot.FourBet:
                                finalDecision = new CAction(ActionsPossible.Fold);
                                break;
                            case TypesPot.FiveBetEtPlus:
                                finalDecision = new CAction(ActionsPossible.Fold);
                                break;
                        }

                        FFVillainRange = GetRangeFromAction(_simulatorThatRepresentCurrentGame.GetTableInformationsFromCurrentPlayerPointOfView(), null);
                        break;
                    case ToursPossible.Flop:
                        finalDecision = new CAction(ActionsPossible.Check);
                        break;
                    case ToursPossible.Turn:
                        finalDecision = new CAction(ActionsPossible.Check);
                        break;
                    case ToursPossible.River: 
                        decimal EVBet25Percent = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, Math.Min(decimal.Multiply(FFTableInfos.PPot, Convert.ToDecimal(0.25)), FFTableInfos.PHero.PNumberOfChipsLeft));
                        decimal EVBet50Percent = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, Math.Min(decimal.Multiply(FFTableInfos.PPot, Convert.ToDecimal(0.50)), FFTableInfos.PHero.PNumberOfChipsLeft));                        
                        decimal EVBet72Percent = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, Math.Min(decimal.Multiply(FFTableInfos.PPot, Convert.ToDecimal(0.72)), FFTableInfos.PHero.PNumberOfChipsLeft));
                        decimal EVBet100Percent = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, Math.Min(decimal.Multiply(FFTableInfos.PPot, Convert.ToDecimal(1)), FFTableInfos.PHero.PNumberOfChipsLeft));
                        decimal EVBet133Percent = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, Math.Min(decimal.Multiply(FFTableInfos.PPot, Convert.ToDecimal(1.33)), FFTableInfos.PHero.PNumberOfChipsLeft));
                        decimal EVBetALLIN = CalculateTotalEVIfWeBet(_simulatorThatRepresentCurrentGame, FFTableInfos.PHero.PNumberOfChipsLeft);
                      //  decimal EVFromChecking = CalculateTotalEVIfWeCheck(_simulatorThatRepresentCurrentGame);
                        
                      /*  Console.WriteLine("EV from betting 25%: " + EVBet25Percent.ToString());
                        Console.WriteLine("EV from betting 50%: " + EVBet50Percent.ToString());
                        Console.WriteLine("EV from betting 72%: " + EVBet72Percent.ToString());
                        Console.WriteLine("EV from betting 100%: " + EVBet100Percent.ToString());
                        Console.WriteLine("EV from betting 133%: " + EVBet133Percent.ToString());
                        Console.WriteLine("EV from betting ALL IN: " + EVBetALLIN.ToString());*/
                        //          Console.WriteLine("EV from checking: " + EVFromChecking.ToString());

                        finalDecision = new CAction(ActionsPossible.Check);
                        break;
                }

                if (finalDecision.PAction == ActionsPossible.Raise && finalDecision.PMise == _headsUpTable.PHero.PNumberOfChipsLeft)
                    CLogger.AddLog(new CLog("Jamming..."));

                #region Logging actions
                FFTimer.Stop();
                
                CLogger.AddEmptyLineLog();

                if (finalDecision.PAction == ActionsPossible.Raise || finalDecision.PAction == ActionsPossible.Bet)
                    CLogger.AddLog(new CLog("FINAL DECISION: " + finalDecision.PAction + " " + finalDecision.PMise + "$ | Time taken: " + FFTimer.ElapsedMilliseconds + "ms"));
                else
                    CLogger.AddLog(new CLog("FINAL DECISION: " + finalDecision.PAction + " | Time taken: " + FFTimer.ElapsedMilliseconds + "ms"));
                #endregion
            }
            catch (Exception e)
            {
                CLogger.AddFooterLog();
                CLogger.AddLog(new CLog("Caught an exception while making a decision. Here's the message: " + e.Message, false));
                throw (e);
            }

            CLogger.AddFooterLog();
            CLogger.AddEmptyLineLog();

            return finalDecision;
        }
        #region Comments
        /*
        private decimal CalculateTotalEVIfWeCheck(ICloneable _currentGameState)
        {            
            CGame2MaxManualController currentGameState = (CGame2MaxManualController)_currentGameState.Clone();
            CTableInfosNLHE2Max tableInfosFromHeroPOV = currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            List<Tuple<CCard, CCard, double>> villainRange = GetRangeFromAction(tableInfosFromHeroPOV);

            decimal totalEV = 0;
            decimal effectivePot = tableInfosFromHeroPOV.GetEffectivePot();
            
            foreach (Tuple<CCard, CCard, double> currentCards in villainRange)
            {
                CGame2MaxManualController simulatedGameState = (CGame2MaxManualController)currentGameState.Clone();
                Tuple<CCard, CCard> currentVillainHand = new Tuple<CCard, CCard>(currentCards.Item1, currentCards.Item2);

                simulatedGameState.Check();

                decimal highestEVFromVillainWithCurrentHand = GetHighestEVFromVillainIfWeCheck(simulatedGameState, currentVillainHand);
                decimal highestEVIncludingProbabilityThatVillainHasIt = decimal.Multiply(highestEVFromVillainWithCurrentHand, Convert.ToDecimal(currentCards.Item3));
                
                totalEV += (effectivePot - highestEVIncludingProbabilityThatVillainHasIt);
            }

            return decimal.Divide(totalEV, villainRange.Count);
        }

        private decimal GetHighestEVFromVillainIfWeCheck(CGame2MaxManualController _currentGameState, Tuple<CCard, CCard> _villainHand)
        {
            HashSet<ActionsPossible> allowedActions = _currentGameState.GetAllowedActions();

            if (!allowedActions.Contains(ActionsPossible.Check))
                throw new InvalidOperationException("We should be able to atleast check because we're on the river and there's a last bet! Something wrong happened!");

            CTableInfosNLHE2Max tableInfosFromHeroPOV = _currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            List<Tuple<CCard, CCard, double>> villainRange = GetRangeFromAction(tableInfosFromHeroPOV);

            decimal EVFromChecking = 0;
            decimal EVFromBetting25Percent = 0;
            decimal potIfWeWin = tableInfosFromHeroPOV.GetEffectivePot();
            string currentBoard = tableInfosFromHeroPOV.PBoard;

            Hand heroCardsWithBoard = new Hand(_villainHand.Item1.ToString() + " " + _villainHand.Item2.ToString(), currentBoard);

           /* decimal EVFromBetting50Percent = 0;
            decimal EVFromBetting72Percent = 0;
            decimal EVFromBetting100Percent = 0;
            decimal EVFromBetting133Percent = 0;
            decimal EVFromBettingAllIn = 0;


            foreach (Tuple<CCard, CCard, double> currentCards in villainRange)
            {
                Hand currentVillainHand = new Hand(currentCards.Item1.ToString() + " " + currentCards.Item2.ToString(), heroCardsWithBoard.Board);

                if (heroCardsWithBoard > currentVillainHand)
                    EVFromChecking += (potIfWeWin * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
                else if (heroCardsWithBoard == currentVillainHand)
                    EVFromChecking += (decimal.Divide(potIfWeWin, 2) * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards

                decimal betSize25Percent = decimal.Multiply(tableInfosFromHeroPOV.PPot, Convert.ToDecimal(0.25));

                EVFromBetting25Percent += CalculateTotalEVIfWeBet(_currentGameState, betSize25Percent);
            }

            decimal bestEV = Math.Max(EVFromChecking, EVFromBetting25Percent);

            if (bestEV > 0)
                return decimal.Divide(bestEV, villainRange.Count);
            else
                return 0; 
        }
        */
        #endregion
        private decimal CalculateTotalEVIfWeBet(ICloneable _currentGameState, decimal _betThatWeNeedToPut)
        {
            CGame2MaxManualController simulatedGameState = (CGame2MaxManualController)_currentGameState.Clone();
            List<CComboCard> villainRangeAtRiver = GetRangeFromAction(simulatedGameState.GetTableInformationsFromCurrentPlayerPointOfView(), FFVillainRange); // TO BE CHANGED: Normally we should give the turn range of villain
            Hand heroHand = new Hand(FFHeroCards, FFTableInfos.PBoard);

            simulatedGameState.Bet(_betThatWeNeedToPut);
            decimal OurEVIfVillainCallOrFoldFacingOurBet = CalculateTotalEVIfWeBetAndVillainCallOrFold(simulatedGameState, ref villainRangeAtRiver, heroHand, _betThatWeNeedToPut);

            HashSet<ActionsPossible> possibleActions = simulatedGameState.GetAllowedActions();

            if (possibleActions.Contains(ActionsPossible.Raise))
            {
                #region Setting up infos for raising
                CTableInfosNLHE2Max tableInfos = simulatedGameState.GetTableInformationsFromCurrentPlayerPointOfView();
                decimal heroNumberOfChipsLeft = tableInfos.PHero.PNumberOfChipsLeft; // Number of chips from villain (yes, really from villain)
                decimal raiseSize2x = Math.Min(heroNumberOfChipsLeft, decimal.Multiply(_betThatWeNeedToPut, 2));
                #endregion

                simulatedGameState.Raise(raiseSize2x);
                decimal OurEVIfVillainRaiseFacingOurBet = CalculateTotalEVIfVillainRaise(simulatedGameState, ref villainRangeAtRiver, heroHand, _betThatWeNeedToPut);

                return decimal.Add(OurEVIfVillainCallOrFoldFacingOurBet, OurEVIfVillainRaiseFacingOurBet);
            }
            else                            
                return OurEVIfVillainCallOrFoldFacingOurBet;                           
        }

        private decimal CalculateTotalEVIfWeBetAndVillainCallOrFold(CGame2MaxManualController _currentGameState, ref List<CComboCard> _villainRange, Hand _heroCards, decimal _initialBet)
        {
            CGame2MaxManualController simulatedGameState = (CGame2MaxManualController)_currentGameState.Clone();
            decimal potIfVillainFold = (simulatedGameState.GetTableInformationsFromCurrentPlayerPointOfView().PPot - _initialBet);
            simulatedGameState.Call();

            CTableInfosNLHE2Max tableInfos = simulatedGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            decimal totalEV = 0;
            decimal potIfVillainCall = tableInfos.GetEffectivePot();
            
            _villainRange = GetRangeFromAction(tableInfos, _villainRange);

            foreach (CComboCard currentComboCards in _villainRange)
            {                
                Hand currentVillainCards = new Hand(currentComboCards.PCard1.ToString() + " " + currentComboCards.PCard2.ToString(), tableInfos.PBoard);
                decimal probabilityVillainCall = currentComboCards.PProbabilityCallingCurrentStreet;
                decimal probabilityVillainFold = currentComboCards.PProbabilityFoldingCurrentStreet;

                if (_heroCards > currentVillainCards)
                    totalEV += decimal.Multiply(potIfVillainCall, probabilityVillainCall); // Item3 = probability that the villain has the cards
                else if (_heroCards < currentVillainCards)
                    totalEV -= decimal.Multiply(_initialBet, probabilityVillainCall); // Item3 = probability that the villain has the cards
                else
                    totalEV += decimal.Multiply(decimal.Divide(potIfVillainCall, 2), probabilityVillainCall); // Item3 = probability that the villain has the cards

                totalEV += decimal.Multiply(potIfVillainFold, probabilityVillainFold);
            }

            return decimal.Divide(totalEV, _villainRange.Count);
        }

        private decimal CalculateTotalEVIfVillainRaise(CGame2MaxManualController _currentGameState, ref List<CComboCard> _villainRange, Hand _heroCards, decimal _initialBet)
        {            
            CTableInfosNLHE2Max tableInfos = _currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            CPlayer hero = tableInfos.PHero;
            decimal EVFromCalling = (0 - _initialBet);
            decimal EVFromFolding = (0 - _initialBet);
            decimal potIfWeCall = (Math.Min(hero.PNumberOfChipsLeft, tableInfos.PVillain.PLastBet - hero.PLastBet) + tableInfos.GetEffectivePot());
            decimal probabilityOfVillainRaising = 0;
            _villainRange = GetRangeFromAction(tableInfos, _villainRange);

            foreach (CComboCard currentCards in _villainRange)
            {
                Hand currentVillainCards = new Hand(currentCards.PCard1.ToString() + " " + currentCards.PCard2.ToString(), tableInfos.PBoard);
                decimal probabilityVillainRaise = currentCards.PProbabilityRaisingCurrentStreet;

                if (_heroCards > currentVillainCards)
                    EVFromCalling += (potIfWeCall * probabilityVillainRaise); // Item3 = probability that the villain has the cards
                else if (_heroCards < currentVillainCards)
                    EVFromCalling -= (_initialBet * probabilityVillainRaise); // Item3 = probability that the villain has the cards
                else
                    EVFromCalling += (decimal.Divide(potIfWeCall, 2) * probabilityVillainRaise); // Item3 = probability that the villain has the cards

                probabilityOfVillainRaising += probabilityVillainRaise;
            }

            probabilityOfVillainRaising = decimal.Divide(probabilityOfVillainRaising, _villainRange.Count);
            EVFromCalling = decimal.Divide(EVFromCalling, _villainRange.Count);
            
            decimal bestEVAaction = Math.Max(EVFromCalling, EVFromFolding);  

            return decimal.Multiply(probabilityOfVillainRaising, bestEVAaction);
        }
        #region Comments
        /*
        private decimal CalculateTotalEVIfWeCall(CGame2MaxManualController _currentGameState)
        {
            CTableInfosNLHE2Max tableInfos = _currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            decimal totalEV = 0;
            decimal potIfWeWin = tableInfos.GetEffectivePot();
            List<Tuple<CCard, CCard, double>> villainRange = GetRangeFromAction(tableInfos);

            foreach (Tuple<CCard, CCard, double> currentCards in villainRange)
            {
                Hand currentVillainCards = new Hand(currentCards.Item1.ToString() + " " + currentCards.Item2.ToString(), tableInfos.PBoard);

                if (_heroCards > currentVillainCards)
                    totalEV += (potIfWeWin * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
                else if (_heroCards < currentVillainCards)
                    totalEV -= (_initialBet * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
                else
                    totalEV += (decimal.Divide(potIfWeWin, 2) * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
            }

            return decimal.Divide(totalEV, villainRange.Count);
        }
        private decimal GetHighestEVFromVillainIfWeBet(decimal _betSize, CGame2MaxManualController _currentGameState, Tuple<CCard, CCard> _villainHand)
        {
            HashSet<ActionsPossible> allowedActions = _currentGameState.GetAllowedActions();

            if (!allowedActions.Contains(ActionsPossible.Call))
                throw new InvalidOperationException("We should be able to atleast call because we're on the river and there's a last bet! Something wrong happened!");

            CTableInfosNLHE2Max tableInfosFromHeroPOV = _currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            Hand heroCardsWithBoard = new Hand(_villainHand.Item1.ToString() + " " + _villainHand.Item2.ToString(), tableInfosFromHeroPOV.PBoard);

            decimal EVFromCalling = CalculateTotalEVIfWeCall(_betSize, _currentGameState, heroCardsWithBoard);
            decimal EVFromRaising2x = -1;

            if (allowedActions.Contains(ActionsPossible.Raise))
            {
                decimal heroNumberOfChipsLeft = tableInfosFromHeroPOV.PHero.PNumberOfChipsLeft;
                decimal raiseSize2x = decimal.Multiply(_betSize, 2);

                if (raiseSize2x > heroNumberOfChipsLeft)
                    EVFromRaising2x = CalculateTotalEVIfWeRaise(tableInfosFromHeroPOV.PHero.PNumberOfChipsLeft, _currentGameState, heroCardsWithBoard);
                else
                    EVFromRaising2x = CalculateTotalEVIfWeRaise(raiseSize2x, _currentGameState, heroCardsWithBoard);
            }  
                

            decimal bestEV = Math.Max(EVFromCalling, EVFromRaising2x);

            if (bestEV > 0)
                return bestEV;
            else
                return 0; // Villain is better folding than calling
            
        }

        private decimal CalculateTotalEVIfWeCall(decimal _betThatWeNeedToPut, CGame2MaxManualController _currentGameState, Hand _heroCardsWithBoard)
        {
            CGame2MaxManualController currentGameState = (CGame2MaxManualController)_currentGameState.Clone();
            CTableInfosNLHE2Max tableInfosFromHeroPOV = currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            List<Tuple<CCard, CCard, double>> villainRange = GetRangeFromAction(tableInfosFromHeroPOV);

            decimal totalEV = 0;
            decimal betThatWeNeedToPut = (_betThatWeNeedToPut > tableInfosFromHeroPOV.PHero.PNumberOfChipsLeft) ? tableInfosFromHeroPOV.PHero.PNumberOfChipsLeft : _betThatWeNeedToPut;
            decimal potIfWeWin = (betThatWeNeedToPut + tableInfosFromHeroPOV.GetEffectivePot());

            //

            foreach (Tuple<CCard, CCard, double> currentCards in villainRange)
            {
                Hand currentVillainHand = new Hand(currentCards.Item1.ToString() + " " + currentCards.Item2.ToString(), _heroCardsWithBoard.Board);

                if (_heroCardsWithBoard > currentVillainHand)
                    totalEV += (potIfWeWin * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
                else if (_heroCardsWithBoard < currentVillainHand)
                    totalEV -= (betThatWeNeedToPut * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
                else
                    totalEV += (decimal.Divide(potIfWeWin, 2) * Convert.ToDecimal(Math.Round(currentCards.Item3, 2))); // Item3 = probability that the villain has the cards
            }

            return decimal.Divide(totalEV, villainRange.Count);
        }

        private decimal CalculateTotalEVIfWeRaise(decimal _betThatWeNeedToPut, CGame2MaxManualController _currentGameState, Hand _heroCardsWithBoard)
        {
            CGame2MaxManualController currentGameState = (CGame2MaxManualController)_currentGameState.Clone();
            CTableInfosNLHE2Max tableInfosFromHeroPOV = currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            List<Tuple<CCard, CCard, double>> villainRange = GetRangeFromAction(tableInfosFromHeroPOV);

            decimal effectivePotIfVillainFolds = tableInfosFromHeroPOV.GetEffectivePot();
            decimal totalEV = 0;
            decimal nbFold = 0;
            decimal nbGame = 0;

            foreach (Tuple<CCard, CCard, double> currentCards in villainRange)
            {
                CGame2MaxManualController simulatedGameState = (CGame2MaxManualController)_currentGameState.Clone();
                Tuple<CCard, CCard> currentVillainHand = new Tuple<CCard, CCard>(currentCards.Item1, currentCards.Item2);
                
                simulatedGameState.Raise(_betThatWeNeedToPut);

                decimal highestEVFromVillainWithCurrentHand = GetHighestEVFromVillainIfWeRaise(_betThatWeNeedToPut, simulatedGameState, currentVillainHand);
                decimal highestEVIncludingProbabilityThatVillainHasIt = decimal.Multiply(highestEVFromVillainWithCurrentHand, Convert.ToDecimal(currentCards.Item3));

                CTableInfosNLHE2Max tableInfos = simulatedGameState.GetTableInformationsFromCurrentPlayerPointOfView();
                decimal effectivePot = tableInfos.GetEffectivePot();

                // It means that villain is just better to fold
                if (highestEVIncludingProbabilityThatVillainHasIt == 0)
                {
                    totalEV += effectivePotIfVillainFolds;
                    ++nbFold;
                }
                else
                    totalEV += (effectivePot - highestEVIncludingProbabilityThatVillainHasIt);                

                ++nbGame;
            }

     //       Console.WriteLine("Fold equity if we raise " + _betThatWeNeedToPut.ToString() + "$ : " + Math.Round(Decimal.Multiply(Decimal.Divide(nbFold, nbGame), 100), 2).ToString() + "%");
            return decimal.Divide(totalEV, villainRange.Count);        
        }

        private decimal GetHighestEVFromVillainIfWeRaise(decimal _raiseSize, CGame2MaxManualController _currentGameState, Tuple<CCard, CCard> _villainHand)
        {
            HashSet<ActionsPossible> allowedActions = _currentGameState.GetAllowedActions();

            if (!allowedActions.Contains(ActionsPossible.Call))
                throw new InvalidOperationException("We should be able to atleast call because we're on the river and there's a last raise! Something wrong happened!");

            CTableInfosNLHE2Max tableInfosFromHeroPOV = _currentGameState.GetTableInformationsFromCurrentPlayerPointOfView();
            string currentBoard = tableInfosFromHeroPOV.PBoard;
            decimal lastBet = tableInfosFromHeroPOV.PHero.PLastBet;
            Hand heroCardsWithBoard = new Hand(_villainHand.Item1.ToString() + " " + _villainHand.Item2.ToString(), currentBoard);

            decimal EVFromCalling = CalculateTotalEVIfWeCall(_raiseSize - lastBet, _currentGameState, heroCardsWithBoard);
            //  decimal EVFromRaising2x = CalculateTotalEVIfWeRaise(decimal.Multiply(_raiseSize, 2), _simulator, heroCardsWithBoard, villainRange);

            decimal bestEV = EVFromCalling;

            if (bestEV > 0)
                return bestEV;
            else
                return 0; // Villain is better folding than calling
        }
        */
        #endregion
        private List<CComboCard> GetRangeFromAction(CTableInfosNLHE2Max _tableInfos, List<CComboCard> _oldRange)
        {
            List<string> board = new List<string>(_tableInfos.PBoard.Split(' '));
            List<CComboCard> range = new List<CComboCard>();

            #region Création de la liste des cartes
            List<CCard> cardList = new List<CCard>();

            cardList.Add(new CCard(CCard.Value.Ace, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Ace, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Ace, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Ace, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Two, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Two, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Two, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Two, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Three, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Three, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Three, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Three, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Four, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Four, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Four, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Four, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Five, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Five, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Five, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Five, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Six, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Six, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Six, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Six, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Seven, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Seven, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Seven, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Seven, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Eight, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Eight, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Eight, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Eight, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Nine, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Nine, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Nine, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Nine, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Ten, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Ten, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Ten, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Ten, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Jack, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Jack, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Jack, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Jack, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.Queen, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.Queen, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.Queen, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.Queen, CCard.Type.Clubs));
            cardList.Add(new CCard(CCard.Value.King, CCard.Type.Spades));
            cardList.Add(new CCard(CCard.Value.King, CCard.Type.Hearts));
            cardList.Add(new CCard(CCard.Value.King, CCard.Type.Diamonds));
            cardList.Add(new CCard(CCard.Value.King, CCard.Type.Clubs));
            #endregion

            for (int i = 0; i < cardList.Count - 1; ++i)
            {
                CCard currentCard = cardList[i];

                if (!board.Contains(currentCard.ToString()))
                {
                    for (int j = i + 1; j < cardList.Count; ++j)
                    {
                        CCard secondCard = cardList[j];

                        if (!board.Contains(secondCard.ToString()))
                            range.Add(new CComboCard(currentCard, secondCard, 0, 0, 0.4m, 0.4m, 0.2m));
                    }
                }
            }

            return range;            
        }
        
        private List<CComboCard> GetRangeOnRiverFromAction(CTableInfosNLHE2Max _tableInfos, List<CComboCard> _oldRange)
        {
            if (FFCurrentStreet != ToursPossible.River)
                throw new InvalidOperationException("GetRangeOnRiverFromAction just got called, but we are not on the river!");

            return _oldRange;
        }
        /*
        private List<CComboCard> GetRangeOnTurnFromAction(CTableInfosNLHE2Max _tableInfos, List<CComboCard> _oldRange)
        {
            if (FFCurrentStreet != ToursPossible.Turn)
                throw new InvalidOperationException("GetRangeOnTurnFromAction just got called, but we are not on the turn!");

            Hand heroCards = new Hand(FFHeroCards, _tableInfos.PBoard);

            switch (_tableInfos.PHero.PPosition)
            {
                case PossiblePositions.BB:
                    #region 2bet pot
                    #region BB checks the turn
                    #endregion
                    break;
                case PossiblePositions.BTN:
                    #region 2bet pot
                    #region BB bets
                    #endregion
                    #region BB checks
                    int currentVillainCardsIndex = 0;

                    while (currentVillainCardsIndex < _oldRange.Count)
                    {
                        CComboCard currentVillainCards = _oldRange[currentVillainCardsIndex];
                        Hand villainCards = _oldRange[currentVillainCardsIndex].ToHoldemHand();

                        //currentVillainCards.PCard1.
                        Hand.
                        ++currentVillainCardsIndex;
                    }
                    // 2ième paire qui ont été fait sur la turn
                    #endregion
                    #region 3bet pot
                    #region 4bet pot
                    #endregion
                    break;
                default:
                    throw new InvalidOperationException("Unable to detect the position of hero. The bot cannot take a decision.");
            }
        }

        private List<CComboCard> GetRangeOnFlopFromAction(CTableInfosNLHE2Max _tableInfos, List<CComboCard> _oldRange)
        {
            if (FFCurrentStreet != ToursPossible.Flop)
                throw new InvalidOperationException("GetRangeOnFlopFromAction just got called, but we are not on the flop!");

            Hand heroCards = new Hand(FFHeroCards, _tableInfos.PBoard);

            switch (_tableInfos.PHero.PPosition)
            {
                case PossiblePositions.BB:
                    #region 2bet pot
                    #region BTN cbets the flop.
                    int currentVillainCardsIndex = 0;

                    while (currentVillainCardsIndex < _oldRange.Count)
                    {
                        CComboCard currentVillainCards = _oldRange[currentVillainCardsIndex];
                        Hand villainCards = _oldRange[currentVillainCardsIndex].ToHoldemHand();

                        bool dontRemoveCardFromRange = true;

                        if (Hand.IsFlushDraw(villainCards.PocketMask, heroCards.BoardMask, 0L))
                        {
                            // If he has a pair + flush draw
                            if (villainCards.HandTypeValue == Hand.HandTypes.Pair)
                            {
                                currentVillainCards.PProbabilityCheckingCurrentStreet = 0.7m;
                                currentVillainCards.PProbabilityBettingCurrentStreet = 0.3m;
                            }
                            else
                            {
                                if (villainCards.HandTypeValue == Hand.HandTypes.HighCard)
                                {
                                    // Ace high flush draws - to do
                                    // Other flush draws - to do
                                    //currentVillainCards.PProbabilityBettingCurrentStreet = 0.8;
                                }
                                
                            }
                                
                        }

                        ++currentVillainCardsIndex;
                    }
                    
                    #endregion
                    #region We donk the flop, BTN call.
                    #endregion
                    #region We check-raised the flop, BTN call
                    #endregion
                    #region We check-raised the flop, BTN re-raised
                    #endregion
                    #endregion
                    #region 3bet pot
                    #region We cbet and BTN raises.
                    #endregion
                    #region We cbet and BTN calls
                    #endregion
                    #region We check and BTN bets
                    #endregion
                    #region We check and BTN checks back
                    #endregion
                    #endregion
                    #region 4bet pot
                    #region BTN cbets the flop.
                    #endregion
                    #region We check-raised vs BTN cbet, BTN calls
                    #endregion
                    #region We check-raised vs BTN cbet, BTN raises
                    #endregion
                    #endregion
                    break;
                case PossiblePositions.BTN:
                    #region 2bet pot
                    #region BB donks
                    #endregion
                    #region 3bet pot
                    #region 4bet pot
                    #endregion
                    break;
                default:
                    throw new InvalidOperationException("Unable to detect the position of hero. The bot cannot take a decision.");
            }
        }
        */
        /// <summary>
        /// Calculate preflop equity based on a range and a hand given
        /// </summary>
        /// <returns></returns>        
        private decimal CalculatePreflopRealizedEquity(CCard _heroCard1, CCard _heroCard2, string[] _villainRange, bool _heroIsInPosition)
        {
            const int MAXIMUM_TRIAL = 200000;

            FFTimer.Restart();
            CLogger.AddHeaderLog();
            CLogger.AddLog(new CLog("Calculating preflop REALIZED equity..."));

            string heroCard1 = _heroCard1.ToString();
            string heroCard2 = _heroCard2.ToString();

            CLogger.AddLog(new CLog("Hero cards: " + _heroCard1 + _heroCard2));

            long heroWins = 0;
            long ties = 0;
            long count = 0;

            ulong heroMask = Hand.ParseHand(heroCard1 + heroCard2);

            foreach(string villainCurrentHand in _villainRange)
            {
                string firstVillainCard = (villainCurrentHand.Substring(0, 2));
                string secondVillainCard = (villainCurrentHand.Substring(2, 2));

                if (!(heroCard1 == firstVillainCard || heroCard1 == secondVillainCard ||
                      heroCard2 == firstVillainCard || heroCard2 == secondVillainCard))
                {
                    ulong villainMask = Hand.ParseHand(villainCurrentHand);

                    IEnumerable<ulong> enumBoardMasks = Hand.RandomHands(0L, heroMask | villainMask, 5, (MAXIMUM_TRIAL / _villainRange.Length));
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
                else
                    CLogger.AddLog(new CLog("Skipped combinaison: " + villainCurrentHand + " because we have the hand " + (heroCard1 + heroCard2)));
            }
            
            CLogger.AddLog(new CLog("HeroWins: " + heroWins));
            CLogger.AddLog(new CLog("Ties: " + ties));
            CLogger.AddLog(new CLog("Count: " + count));

            decimal preflopEquity = Convert.ToDecimal(Math.Round((((double)heroWins) + ((double)ties) / 2.0) / ((double)count) * 100.0, 2));

            CLogger.AddLog(new CLog("Preflop raw equity: " + preflopEquity + "%"));

            if (IsOffSuit(_heroCard1, _heroCard2))
            {
                CLogger.AddLog(new CLog("IsOffSuit: True | -2%"));
                preflopEquity -= 2;
            }
            else
                CLogger.AddLog(new CLog("IsOffSuit: False | Equity is not changed"));

            int gapCount = Hand.GapCount(heroMask);

            if (gapCount == 0)
            {
                CLogger.AddLog(new CLog("IsConnected (GapCount = 0): True | +1.5%"));
                preflopEquity += new decimal(1.5);
            }                
            else if (gapCount == 1)
            {
                CLogger.AddLog(new CLog("GapCount: 1 | +0.7%"));
                preflopEquity += new decimal(0.7);
            }
            else
                CLogger.AddLog(new CLog("GapCount: " + gapCount + " | Equity is not changed"));

            if (_heroIsInPosition)
            {
                CLogger.AddLog(new CLog("Hero is in position: True | +2.5%"));
                preflopEquity += new decimal(2.5);
            }                
            else
            {
                CLogger.AddLog(new CLog("Hero is in position: False | -2.5%"));
                preflopEquity -= new decimal(2.5);
            }            

            FFTimer.Stop();

            CLogger.AddEmptyLineLog();
            CLogger.AddLog(new CLog("Preflop REALIZED equity: " + preflopEquity + "%" + " | Time taken: " + FFTimer.Elapsed.TotalMilliseconds + "ms"));
            CLogger.AddFooterLog();

            return preflopEquity;
        }

        private decimal CalculatePreflopRawEquity(CCard _heroCard1, CCard _heroCard2, string[] _villainRange)
        {
            const int MAXIMUM_TRIAL = 200000;

            FFTimer.Restart();
            CLogger.AddHeaderLog();
            CLogger.AddLog(new CLog("Calculating preflop RAW equity..."));

            string heroCard1 = _heroCard1.ToString();
            string heroCard2 = _heroCard2.ToString();

            CLogger.AddLog(new CLog("Hero cards: " + _heroCard1 + _heroCard2));

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

                    IEnumerable<ulong> enumBoardMasks = Hand.RandomHands(0L, heroMask | villainMask, 5, (MAXIMUM_TRIAL / _villainRange.Length));
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
                else
                    CLogger.AddLog(new CLog("Skipped combinaison: " + villainCurrentHand + " because we have the hand " + (heroCard1 + heroCard2)));
            }

            CLogger.AddLog(new CLog("HeroWins: " + heroWins));
            CLogger.AddLog(new CLog("Ties: " + ties));
            CLogger.AddLog(new CLog("Count: " + count));

            decimal preflopEquity = Convert.ToDecimal(Math.Round((((double)heroWins) + ((double)ties) / 2.0) / ((double)count) * 100.0, 2));
            FFTimer.Stop();

            CLogger.AddEmptyLineLog();
            CLogger.AddLog(new CLog("Preflop RAW equity: " + preflopEquity + "%" + " | Time taken: " + FFTimer.Elapsed.TotalMilliseconds + "ms"));
            CLogger.AddFooterLog();

            return preflopEquity;
        }

        private bool IsOffSuit(CCard _heroCard1, CCard _heroCard2)
        {
            return (_heroCard1.PType != _heroCard2.PType);
        }
    }
}
