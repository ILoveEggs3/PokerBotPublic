using Amigo.Core;
using Amigo.Models;
using System;
using System.Collections.Generic;

using static Amigo.Models.CAction;

namespace Amigo.Core
{
    public class CTableInfosNLHE2Max : CTableInfos
    {
        #region Données membres, accesseurs, constantes et énumérés        

        /// <summary>
        /// Joueur adversaire de la table en cours.
        /// </summary>
        public CPlayer PVillain { private set; get; }

        public CPlayer PHero { private set; get; }

        /// <summary>
        /// Historique des actions de la main actuel en train de se jouer.
        /// </summary>
        private List<Tuple<CAction, bool>> FFLstActionsMainActuel;
        
        #endregion
        #region Constructeurs
        public CTableInfosNLHE2Max(decimal _smallBlind, decimal _bigBlind, decimal _antes, decimal _pot, CPlayer _hero, Tuple<CCard, CCard> _heroCards, string _board) : base(_smallBlind, _bigBlind, _antes, _pot, _board, _heroCards)
        {
            if (_hero == null)
                throw new ArgumentNullException("_hero");

            FFLstActionsMainActuel = new List<Tuple<CAction, bool>>();
            PHero = _hero;
        }

        public CTableInfosNLHE2Max(decimal _smallBlind, decimal _bigBlind, decimal _antes, decimal _pot, CPlayer _hero, Tuple<CCard, CCard> _heroCards, string _board, CPlayer _villain) : base(_smallBlind, _bigBlind, _antes, _pot, _board, _heroCards)
        {
            if (_hero == null)
                throw new ArgumentNullException("_hero");
            else if (_villain == null)
                throw new ArgumentNullException("_villain");

            FFLstActionsMainActuel = new List<Tuple<CAction, bool>>();
            PHero = _hero;
            PVillain = _villain;
        }
        #endregion

        /// <summary>
        /// Add an action to the history of actions
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_actionIsFromHero">If the action is done by the hero. If it's done by the villain, this should be false.</param>
        public void AjouterAction(CAction _action, bool _actionIsFromHero)
        {
            FFLstActionsMainActuel.Add(new Tuple<CAction, bool>(_action, _actionIsFromHero));
        }

        public override ToursPossible GetTourActuel()
        {
            int numberOfStreetChanged = 0;
            bool previousActionWasACheck = false;

            if (FFLstActionsMainActuel.Count > 1)
            {
                bool SBCalledPreflop = FFLstActionsMainActuel[0].Item1.PAction == CAction.ActionsPossible.Call;
                bool BBCheckedPreflop = FFLstActionsMainActuel[1].Item1.PAction == CAction.ActionsPossible.Check;

                // Exception case: Sinon ça fuck up l'algorithme pour les checks
                if (SBCalledPreflop)
                {
                    for (int i = 2; i < FFLstActionsMainActuel.Count; ++i)
                    {
                        CAction action = FFLstActionsMainActuel[i].Item1;

                        if (action.PAction == CAction.ActionsPossible.Call)                        
                            ++numberOfStreetChanged;                                                    
                        else if (action.PAction == CAction.ActionsPossible.Check)
                        {
                            if (previousActionWasACheck)
                            {
                                ++numberOfStreetChanged;
                                previousActionWasACheck = false;
                            }
                            else
                                previousActionWasACheck = true;
                        }
                        else
                            previousActionWasACheck = false;
                    }
                    
                    if (BBCheckedPreflop)
                    {
                        switch (numberOfStreetChanged)
                        {
                            case 0:
                                return ToursPossible.Flop;
                            case 1:
                                return ToursPossible.Turn;
                            case 2:
                                return ToursPossible.River;
                            default:
                                throw new Exception("Unable to detect the street (preflop, flop, turn or river)");
                        }
                    }
                    else
                    {
                        switch (numberOfStreetChanged)
                        {
                            case 0:
                                return ToursPossible.Preflop;
                            case 1:
                                return ToursPossible.Flop;
                            case 2:
                                return ToursPossible.Turn;
                            case 3:
                                return ToursPossible.River;
                            default:
                                throw new Exception("Unable to detect the street (preflop, flop, turn or river)");
                        }
                    }
                }
                else
                {
                    for(int indAction = 0; indAction < FFLstActionsMainActuel.Count; ++indAction)
                    {
                        CAction action = FFLstActionsMainActuel[indAction].Item1;

                        if (action.PAction == ActionsPossible.Call)
                            ++numberOfStreetChanged;
                        else if (action.PAction == ActionsPossible.Check)
                        {
                            if (previousActionWasACheck)
                            {
                                ++numberOfStreetChanged;
                                previousActionWasACheck = false;
                            }
                            else
                                previousActionWasACheck = true;
                        }
                        else
                            previousActionWasACheck = false;
                    }

                    switch (numberOfStreetChanged)
                    {
                        case 0:
                            return ToursPossible.Preflop;
                        case 1:
                            return ToursPossible.Flop;
                        case 2:
                            return ToursPossible.Turn;
                        case 3:
                            return ToursPossible.River;
                        default:
                            throw new Exception("Unable to detect the street (preflop, flop, turn or river)");
                    }
                }
            }
            else
                return ToursPossible.Preflop;
        }

        public override TypesPot GetTypePot()
        {
            int numberOfRaises = 0;

            if (FFLstActionsMainActuel.Count >= 2)
            {
                if (FFLstActionsMainActuel[0].Item1.PAction == CAction.ActionsPossible.Call)
                {
                    // Le limper limp. On check.
                    if (FFLstActionsMainActuel[1].Item1.PAction == CAction.ActionsPossible.Check)
                        return TypesPot.Limped;
                    // Le limper limp. On raise.
                    else if (FFLstActionsMainActuel[1].Item1.PAction == CAction.ActionsPossible.Raise)
                    {
                        if (FFLstActionsMainActuel.Count >= 3)
                        {
                            // Le limper 3bet notre raise
                            if (FFLstActionsMainActuel[2].Item1.PAction == CAction.ActionsPossible.Raise)
                            {
                                // On 4bet le limper qui a 3bet
                                if (FFLstActionsMainActuel[3].Item1.PAction == CAction.ActionsPossible.Raise)
                                {
                                    if (FFLstActionsMainActuel.Count >= 5)
                                    {
                                        // Le limper 5bet notre 4bet
                                        if (FFLstActionsMainActuel[4].Item1.PAction == CAction.ActionsPossible.Raise)
                                            return TypesPot.FiveBetEtPlus;
                                        else
                                            return TypesPot.FourBet;
                                    }
                                    else
                                        return TypesPot.FourBet;
                                }
                                else
                                    return TypesPot.LimpedThreeBet;
                            }
                            else
                                return TypesPot.RaisedLimped;
                        }
                        else
                            return TypesPot.RaisedLimped;
                    }
                    else
                        throw new Exception("We detected that a limper limped. However, we did not detected that we checked or raised, and this should never happen! Did we fold?");

                }
                else
                {
                    for (int indAction = 0; indAction < FFLstActionsMainActuel.Count; ++indAction)
                    {
                        CAction action = FFLstActionsMainActuel[indAction].Item1;

                        if (action.PAction == ActionsPossible.Raise)
                            ++numberOfRaises;
                        else if (action.PAction == ActionsPossible.Call)
                            break;
                    }

                    switch (numberOfRaises)
                    {
                        case 1:
                            return TypesPot.TwoBet;
                        case 2:
                            return TypesPot.ThreeBet;
                        case 3:
                            return TypesPot.FourBet;
                        case 4:
                            return TypesPot.FiveBetEtPlus;
                        default:
                            throw new Exception("Unable to detect the type of pot (1bet, 2bet, 3bet, 4bet, or 5bet+ pot)");
                    }
                }
            }
            else if (FFLstActionsMainActuel.Count == 1)
            {
                if (FFLstActionsMainActuel[0].Item1.PAction == ActionsPossible.Call)
                    return TypesPot.Limped;
                else
                    return TypesPot.TwoBet;
            }
            else
                return TypesPot.OneBet;
        }

        // If hero bet, that means villains called
        // If hero calls, that means either villain bet or raised

        /// <summary>
        /// Gets the last action from villain.
        /// </summary>
        /// <returns>Returns the last action from villain.</returns>
        public CAction GetLastActionFromVillain()
        {
            int indLastAction = (FFLstActionsMainActuel.Count - 1);

            while (indLastAction >= 0)
            {
                if (!FFLstActionsMainActuel[indLastAction].Item2)
                    return FFLstActionsMainActuel[indLastAction].Item1;
                else
                    --indLastAction;
            }

            return null;
        }

        public override decimal GetEffectiveStacksInBB()
        {
            decimal heroBBStack = PHero.ToBB(PBigBlind, true);
            decimal villainBBStack = PVillain.ToBB(PBigBlind, true);

            if (heroBBStack > villainBBStack)
                return villainBBStack;
            else
                return heroBBStack;
        }

        public override decimal GetEffectivePot()
        {            
            if (PVillain.PLastBet > PHero.PNumberOfChipsLeft)            
                return (decimal.Multiply(PHero.PNumberOfChipsAtBeginningHand, 2) - PHero.PNumberOfChipsLeft);                       
            else if (PHero.PLastBet > PVillain.PNumberOfChipsLeft)
                return (decimal.Multiply(PVillain.PNumberOfChipsAtBeginningHand, 2) - PVillain.PNumberOfChipsLeft);
            else
                return PPot;
        }
    }
}
