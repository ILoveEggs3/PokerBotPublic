using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HoldemHand;
using System.IO;
using System.Windows.Forms;
using HandHistories.Objects.Hand;
using Shared.Poker.Models;
using Shared.Models;
using static Shared.Poker.Models.CAction;
using Shared.Helpers;
using static Shared.Poker.Models.CTableInfos;
using Amigo.Interfaces;
using Amigo.Events;

namespace Amigo.Controllers
{
    public abstract class CGameController: IGameController
    {
        /// <summary>
        /// Number of total streets in a texas hold'em game. (Preflop, Flop, Turn, River)
        /// </summary>
        protected const int CCSTREETS_COUNT = 4;
        /// <summary>
        /// Total number of cards in a deck card
        /// </summary>
        protected const int CCTOTAL_CARD_COUNT = 52;

        /// <summary>
        /// Positions possible in poker
        /// </summary>
        protected const int CCSB = 0;
        protected const int CCBB = 1;

        #region Events
        public event EventHandler<COnNewHandEventArgs> POnNewHand;
        public event EventHandler<COnNewActionEventArgs> POnNewAction;
        public event EventHandler<COnNewStreetEventArgs> POnNewStreet;
        public event EventHandler<COnHandFinishedEventArgs> POnHandFinished;
        #endregion

        #region Field members
        #region Protected field members
        /// <summary>
        /// Indice (correspondant à FFTabJoueurs) qui représente le premier joueur à parler postflop, à la prochaine main (et non à la main actuelle)
        /// </summary>
        protected int FFIndPremierJoueurAParlerPostflopProchaineMain;
        
        private int FFIndPremierJoueurAParlerPostflop;
        /// <summary>
        /// Indice du joueur (correspondant à FTabJoueurs) premier à parler postflop
        /// </summary>
        protected int PIndPremierJoueurAParlerPostflop
        {
            set
            {
                if (FFLstJoueursPasFold == null || !FFLstJoueursPasFold.Contains(value))
                    throw new Exception("Valeur invalide. Vérifiez que FLstJoueursPasFold est valide et que la valeur donnée est valide.");

                FFIndPremierJoueurAParlerPostflop = value;
            }
            get { return FFIndPremierJoueurAParlerPostflop; }
        }

        /// <summary>
        /// Indice du dernier joueur (correspondant à FTabJoueurs) à parler au tour actuel (tour étant, soit preflop, flop, turn ou river)
        /// </summary>
        protected int FFIndDernierJoueurAParler;

        protected bool FFResetStacksEveryHand { private set; get; }

        /// <summary>
        /// Représente les cartes des joueurs. L'indice est l'indice des joueurs.
        /// </summary>
        protected string[] FFTabJoueursCartes;

        /// <summary>
        /// Tous les joueurs de la partie
        /// </summary>
        protected CPlayer[] FFTabJoueurs;

        /// <summary>
        /// Contient les indices des joueurs qui n'ont pas encore couché leur carte au tour présent.
        /// </summary>
        protected List<int> FFLstJoueursPasFold;
        /// <summary>
        /// Cartes dans le paquet de carte
        /// </summary>
        protected List<CCard> FFLstCartes;

        /// <summary>
        /// Historique des actions de la main actuel. Int = index of player associated to the action.
        /// </summary>
        protected List<Tuple<CAction, int, Street>> FFLstActionsMainActuel;

        protected Dictionary<int, double> FFStackDepartDebutMain;
        protected Dictionary<int, double> FFStackDepartDebutPartie;

        /// <summary>
        /// Historique des actions de la main actuel par joueur
        /// </summary>
        protected Dictionary<CPlayer, List<CAction>[]> FFLstActionsMainActuelParJoueur;
        #endregion
        #region Public field members

        /// <summary>
        /// Indice (correspondant à FFTabJoueurs) qui représente le joueur à qui est le tour de jouer.
        /// </summary>
        public int PIndJoueurActuel { get; protected set; }

        /// <summary>
        /// Numbers of total hands in the session
        /// </summary>
        public int PHandCount { get; protected set; }

        public double PSmallBlind { get; protected set;  }
        public double PBigBlind { get; protected set; }
        public double PAntes { get; protected set; }
        public double PPot { protected set; get; }
        /// <summary>
        /// Dernière mise du dernier joueur ayant "bet" ou "raise" au cours d'une main.
        /// </summary>
        public double PDerniereMise { get; protected set; }

        public string PBoard { get; protected set; }

        public bool PContinuePlaying { protected set; get; }

        public HashSet<PokerAction> PLstActionsPossibleJoueurActuel
        {
            protected set;
            get;
        }

        private Street FFCurrentStreet;
        /// <summary>
        /// If the current game state is at preflop, flop, turn or river
        /// </summary>
        public Street PCurrentStreet
        {
            protected set
            {
                if (!Enum.IsDefined(typeof(CTableInfos.Street), value))
                    throw new ArgumentException();

                FFCurrentStreet = value;
            }
            get => FFCurrentStreet;
        }

        public Dictionary<string, CSessionInfo> GetSessionInfosForEveryPlayer()
        {
            Dictionary<string, CSessionInfo> dicSessionInfos = new Dictionary<string, CSessionInfo>(FFTabJoueurs.Length);

            foreach (CPlayer _player in FFTabJoueurs)
                dicSessionInfos.Add(_player.PName, _player.PSessionInfo);

            return dicSessionInfos;
        }
        #endregion
        #endregion

        public CGameController(List<CPlayer> _lstPlayers, double _smallBlind, double _bigBlind, double _antes, bool _resetStacksEveryHand)
        {
            #region Local methods
            void LFInitialiserJoueurs()
            {
                // Création des joueurs
                FFTabJoueurs = _lstPlayers.ToArray();
                // Création de l'historique des actions des joueurs
                FFLstActionsMainActuel = new List<Tuple<CAction, int, Street>>();
                FFLstActionsMainActuelParJoueur = new Dictionary<CPlayer, List<CAction>[]>();
                // Création de la liste qui indique les actions possible pour un joueur X
                PLstActionsPossibleJoueurActuel = new HashSet<PokerAction>();
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X
                FFStackDepartDebutMain = new Dictionary<int, double>();
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X au début de la game
                FFStackDepartDebutPartie = new Dictionary<int, double>();

                foreach (CPlayer player in _lstPlayers)
                {
                    FFLstActionsMainActuelParJoueur[player] = new List<CAction>[CCSTREETS_COUNT];
                    FFLstActionsMainActuelParJoueur[player][(int)Street.Preflop] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)Street.Flop] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)Street.Turn] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)Street.River] = new List<CAction>();
                }

                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                    FFStackDepartDebutPartie[indJoueur] = FFTabJoueurs[indJoueur].PNumberOfChipsAtBeginningHand;
            }
            #endregion

            if (_lstPlayers.Count <= 1)
                throw new ArgumentException("_lstPlayers must have atleast 2 players!", "_lstPlayers");

            #region Initialisation des paramètres de la partie
            PSmallBlind = _smallBlind;
            PBigBlind = _bigBlind;
            PAntes = _antes;
            PPot = 0;
            FFResetStacksEveryHand = _resetStacksEveryHand;
            PIndJoueurActuel = -1;
            PContinuePlaying = true;            

            FFIndPremierJoueurAParlerPostflop = -1;
            FFIndPremierJoueurAParlerPostflopProchaineMain = -1;
            FFIndDernierJoueurAParler = -1;
            PBoard = "";

            // Création du tableau des cartes des joueurs
            FFTabJoueursCartes = new string[_lstPlayers.Count];

            // Création de la liste des joueurs qui n'ont pas foldé au cours d'un tour
            FFLstJoueursPasFold = new List<int>();
            #region Création de la liste des cartes
            FFLstCartes = new List<CCard>();

            FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Ace, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Two, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Three, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Four, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Five, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Six, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Seven, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Eight, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Nine, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Ten, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Jack, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.Queen, CCard.Type.Clubs));
            FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Spades));
            FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Hearts));
            FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Diamonds));
            FFLstCartes.Add(new CCard(CCard.Value.King, CCard.Type.Clubs));
            #endregion
            #endregion
            LFInitialiserJoueurs();
        }        

        protected CGameController(CGameController _controller)
        {
            #region Local methods
            Action LFInitialiserJoueurs = () =>
            {
                // Création des joueurs
                FFTabJoueurs = new CPlayer[_controller.FFTabJoueurs.Length];

                int indElem = 0;
                foreach (ICloneable player in _controller.FFTabJoueurs)
                {
                    FFTabJoueurs[indElem] = (CPlayer)player.Clone();
                    indElem++;
                }                    

                // Création de l'historique des actions des joueurs
                FFLstActionsMainActuel = new List<Tuple<CAction, int, Street>>(_controller.FFLstActionsMainActuel.Count);
                foreach (Tuple<CAction, int, Street> currentTuple in FFLstActionsMainActuel)
                    FFLstActionsMainActuel.Add(new Tuple<CAction, int, Street>((CAction)currentTuple.Item1.Clone(), currentTuple.Item2, currentTuple.Item3));

                FFLstActionsMainActuelParJoueur = new Dictionary<CPlayer, List<CAction>[]>(_controller.FFLstActionsMainActuelParJoueur.Count);
                int indElemPlayer = 0;
                foreach(ICloneable currentPlayer in _controller.FFLstActionsMainActuelParJoueur.Keys)
                {
                    List<CAction>[] arrayOfLstActions = new List<CAction>[_controller.FFLstActionsMainActuelParJoueur[(CPlayer)currentPlayer].Length];

                    int indElemAction = 0;
                    foreach(List<CAction> currentLstAction in _controller.FFLstActionsMainActuelParJoueur[(CPlayer)currentPlayer])
                    {
                        arrayOfLstActions[indElemAction] = currentLstAction.Select(item => (CAction)item.Clone()).ToList();
                        indElemAction++;
                    }

                    FFLstActionsMainActuelParJoueur.Add(FFTabJoueurs[indElemPlayer++], arrayOfLstActions);
                }
                    
                // Création de la liste qui indique les actions possible pour un joueur X
                PLstActionsPossibleJoueurActuel = new HashSet<PokerAction>(_controller.PLstActionsPossibleJoueurActuel);
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X
                FFStackDepartDebutMain = new Dictionary<int, double>(_controller.FFStackDepartDebutMain);
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X au début de la game
                FFStackDepartDebutPartie = new Dictionary<int, double>(_controller.FFStackDepartDebutPartie);
            };
            #endregion

            #region Initialisation des paramètres de la partie
            PSmallBlind = _controller.PSmallBlind;
            PBigBlind = _controller.PBigBlind;
            PAntes = _controller.PAntes;
            FFResetStacksEveryHand = _controller.FFResetStacksEveryHand;
            PIndJoueurActuel = _controller.PIndJoueurActuel;
            PContinuePlaying = _controller.PContinuePlaying;
            PCurrentStreet = _controller.PCurrentStreet;
            PPot = _controller.PPot;
            PDerniereMise = _controller.PDerniereMise;

            PIndPremierJoueurAParlerPostflop = _controller.PIndPremierJoueurAParlerPostflop;
            FFIndPremierJoueurAParlerPostflopProchaineMain = _controller.FFIndPremierJoueurAParlerPostflopProchaineMain;
            FFIndDernierJoueurAParler = _controller.FFIndDernierJoueurAParler;
            PBoard = String.Copy(_controller.PBoard);

            // Création du tableau des cartes des joueurs
            FFTabJoueursCartes = new string[_controller.FFTabJoueursCartes.Length];

            int indCard = 0;
            foreach (ICloneable currentCard in _controller.FFTabJoueursCartes)
                FFTabJoueursCartes[indCard++] = (string)currentCard.Clone();            

            // Création de la liste des joueurs qui n'ont pas foldé au cours d'un tour
            FFLstJoueursPasFold = new List<int>(_controller.FFLstJoueursPasFold.Count);

            foreach (int currentIDPlayer in _controller.FFLstJoueursPasFold)
                FFLstJoueursPasFold.Add(currentIDPlayer);

            #region Création de la liste des cartes
            FFLstCartes = new List<CCard>(_controller.FFLstCartes.Count);

            foreach (ICloneable card in _controller.FFLstCartes)
                FFLstCartes.Add((CCard)card.Clone());
            #endregion
            #endregion
            LFInitialiserJoueurs();
        }

        #region Méthodes pour les cartes
        /// <summary>
        /// Distribuer le nombre de cartes désirés à un joueur spécifique.
        /// </summary>
        /// <param name="_indJoueur">Indice du joueur qu'on veut y distribuer des cartes.</param>
        /// <param name="_nbCartesATirer">Nombre de cartes à donner au joueur.</param>
        protected void DistribuerCartes(int _indJoueur, int _nbCartesATirer)
        {
            if (_indJoueur >= FFTabJoueurs.Length ||
                _indJoueur < 0 ||
                _nbCartesATirer <= 0 ||
                _nbCartesATirer > FFLstCartes.Count)
                throw new ArgumentOutOfRangeException();

            int indCarteActuel = 0; // L'indice de la carte tiré "présentement"
            string cartes = null; // Cartes du joueur.

            // Ajoute une carte aléatoire et l'enlève de la possibilité des cartes à tirer. On le fais une fois ici pour éviter de faire plus tard if (carte == null) [...]
            indCarteActuel = CRandomNumberHelper.Between(0, FFLstCartes.Count - 1);
            
            cartes = FFLstCartes.ElementAt(indCarteActuel).ToString();
            FFLstCartes.RemoveAt(indCarteActuel);

            for (int iNbCarte = 1; iNbCarte < _nbCartesATirer; iNbCarte++)
            {
                indCarteActuel = CRandomNumberHelper.Between(0, FFLstCartes.Count - 1);
                cartes = cartes + " " + FFLstCartes.ElementAt(indCarteActuel).ToString();

                // Supprimer la carte dans la liste des cartes.
                FFLstCartes.RemoveAt(indCarteActuel);
            }
            // Remplacement des cartes du joueur par les nouvelles cartes.
            FFTabJoueursCartes[_indJoueur] = cartes;
        }

        /// <summary>
        /// Retourner une carte généré aléatoirement.
        /// </summary>
        /// <returns>Carte généré aléatoirement.</returns>
        protected virtual CCard GetRandomCard()
        {
            int indCarteActuel = 0; // L'indice de la carte tiré
            CCard carte = null; // Représente les cartes du joueur.

            indCarteActuel = CRandomNumberHelper.Between(0, FFLstCartes.Count - 1);
            // Générer une carte aléatoire et l'insérer dans le tableau des cartes des joueurs.
            carte = FFLstCartes.ElementAt(indCarteActuel);

            // Supprimer la carte dans la liste des cartes.
            FFLstCartes.RemoveAt(indCarteActuel);

            // Remplacement des cartes du joueur par les nouvelles cartes.
            return carte;
        }

        #endregion

        public abstract void PlayNewHand();               

        public Task PlayNewHandAsync()
        {
            return Task.Run(() =>
            {
                PlayNewHand();
            });               
        }

        public abstract void StopGame();

        protected abstract void Play();

        protected void RaiseOnNewHandEvent(object sender, COnNewHandEventArgs e)
        {
            POnNewHand?.Invoke(sender, e);
        }

        protected void RaiseOnNewActionEvent(object sender, COnNewActionEventArgs e)
        {
            POnNewAction?.Invoke(sender, e);
        }

        protected void RaiseOnNewStreetEvent(object sender, COnNewStreetEventArgs e)
        {
            POnNewStreet?.Invoke(sender, e);
        }

        protected void RaiseOnHandFinishedEvent(object sender, COnHandFinishedEventArgs e)
        {
            POnHandFinished?.Invoke(sender, e);
        }

        /// <summary>
        /// Event that is called everytime that the something changed in the game (example: A player checked.)
        /// </summary>
        protected abstract void GameStateChanged();

        protected abstract void GameStopped();

        /// <summary>
        /// Event that is called everytime a player made an action.
        /// </summary>
        /// <param name="_action">Action that the player did</param>
        protected abstract void ReceivedAction(CAction _action);
        

        #region Public methods
        /// <summary>
        /// Effectuer l'action de fold pour le joueur à qui est le tour de jouer.
        /// </summary>
        public void Fold()
        {
            ReceivedAction(new CAction(PokerAction.Fold));
        }
        public void Check()
        {
            ReceivedAction(new CAction(PokerAction.Check));
        }

        /// <summary>
        /// Effectuer l'action de call pour le joueur à qui est le tour de jouer.
        /// </summary>
        public void Call()
        {
            ReceivedAction(new CAction(PokerAction.Call, PDerniereMise));
        }

        /// <summary>
        /// Effectuer l'action de miser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>
        public void Bet(double _mise)
        {
            if (_mise > FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft)
                ReceivedAction(new CAction(PokerAction.Bet, FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft));
            else
                ReceivedAction(new CAction(PokerAction.Bet, _mise));
        }

        /// <summary>
        /// Effectuer l'action de raiser pour le joueur à qui est le tour de jouer.
        /// </summary>
        /// <param name="_mise">Mise du joueur à qui est le tour de jouer.</param>
        public void Raise(double _mise)
        {
            if (_mise > (FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft + FFTabJoueurs[PIndJoueurActuel].PLastBet))
                ReceivedAction(new CAction(PokerAction.Raise, FFTabJoueurs[PIndJoueurActuel].PNumberOfChipsLeft + FFTabJoueurs[PIndJoueurActuel].PLastBet));
            else
                ReceivedAction(new CAction(PokerAction.Raise, _mise));
        }
        #endregion
    }
}
