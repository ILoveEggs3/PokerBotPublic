using System;

namespace Amigo.Models
{
    public class CAction: ICloneable
    {
        public enum ActionsPossible { Aucune, Fold, Check, Call, Bet, Raise };

        private ActionsPossible FFAction;
        /// <summary>
        /// Action possible qu'un joueur peut effectuer dans une partie de poker.
        /// </summary>
        public ActionsPossible PAction
        {
            get { return FFAction; }
        }
                    
        private decimal FFMise;
        /// <summary>
        /// Mise du joueur si l'action sélectionné requiert une mise.
        /// </summary>
        public decimal PMise
        {
            get { return FFMise; }
        }     

        /// <summary>
        /// À utiliser si on a une action qui ne nécessite pas de mise.
        /// </summary>
        /// <param name="_action">Action que le joueur a effectué.</param>
        public CAction(ActionsPossible _action)
        {
            if (!Enum.IsDefined(typeof(ActionsPossible), _action))
                throw new ArgumentException();
            else if (_action == ActionsPossible.Bet || _action == ActionsPossible.Raise || _action == ActionsPossible.Call)
                throw new ArgumentException("Action qui nécessite une mise. Veuillez appeler un autre constructeur de la classe CAction.");

            FFAction = _action;
            FFMise = 0;
        }

        /// <summary>
        /// À utiliser si on a une action qui nécessite une mise.
        /// </summary>
        /// <param name="_action">Action que le joueur a effectué.</param>
        /// <param name="_mise">Mise que le joueur a effectué.</param>
        public CAction(ActionsPossible _action, decimal _mise)
        {
            if (!Enum.IsDefined(typeof(ActionsPossible), _action))
                throw new ArgumentException();
            else if (_mise <= 0)
                throw new ArgumentOutOfRangeException("La mise doit être plus grande que 0.");

            FFAction = _action;
            FFMise = _mise;
        }

        private CAction(CAction _action)
        {
            FFAction = _action.FFAction;
            FFMise = _action.FFMise;
        }

        public object Clone()
        {
            return new CAction(this);
        }
    }
}
