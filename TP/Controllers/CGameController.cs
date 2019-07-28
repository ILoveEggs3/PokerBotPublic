using System;
using System.Collections.Generic;
using System.Linq;
using static Amigo.Models.CAction;
using Amigo.Helpers;
using System.Threading.Tasks;
using Amigo.Models;
using static Amigo.Core.CTableInfos;
using System.Threading;
using Amigo.Core;

namespace Amigo.Controllers
{
    public abstract class CGameController
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

        #region Delegates
        public delegate void DGameStopped(CGameController _game);
        #endregion
        #region Events
        public event DGameStopped PEventGameStopped;
        #endregion

        #region Field members
        #region Private field members
        private ToursPossible FFStadeMain;
        private bool FFResetStacksEveryHand;
        #endregion
        #region Protected field members
        /// <summary>
        /// Indice (correspondant à FFTabJoueurs) qui représente le premier joueur à parler postflop
        /// </summary>
        protected int FFIndPremierJoueurAParlerPostflop;
        /// <summary>
        /// Indice (correspondant à FFTabJoueurs) qui représente le premier joueur à parler postflop, à la prochaine main (et non à la main actuelle)
        /// </summary>
        protected int FFIndPremierJoueurAParlerPostflopProchaineMain;
        /// <summary>
        /// Indice du dernier joueur (correspondant à FTabJoueurs) à parler au tour actuel (tour étant, soit preflop, flop, turn ou river)
        /// </summary>
        protected int FFIndDernierJoueurAParler;

        /// <summary>
        /// Représente le board
        /// S'il existe plus que 5 cartes, ce n'est pas normal
        /// </summary>
        protected string FFBoard;

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
        protected List<Tuple<CAction, int>> FFLstActionsMainActuel;

        protected Dictionary<int, decimal> FFStackDepartDebutMain;
        protected Dictionary<int, decimal> FFStackDepartDebutPartie;

        /// <summary>
        /// Historique des actions de la main actuel par joueur
        /// </summary>
        protected Dictionary<CPlayer, List<CAction>[]> FFLstActionsMainActuelParJoueur;

        /// <summary>
        /// Liste d'actions possibles que le joueur à qui est le tour de jouer peut faire.
        /// </summary>
        protected HashSet<ActionsPossible> FLstActionsPossibleJoueurActuel;
        #endregion
        #region Public field members

        /// <summary>
        /// Indice du joueur (correspondant à FTabJoueurs) premier à parler postflop
        /// </summary>
        public int PIndPremierJoueurAParlerPostflop
        {
            protected set
            {
                if (FFLstJoueursPasFold == null || !FFLstJoueursPasFold.Contains(value))
                    throw new Exception("Valeur invalide. Vérifiez que FLstJoueursPasFold est valide et que la valeur donnée est valide.");

                FFIndPremierJoueurAParlerPostflop = value;
            }
            get { return FFIndPremierJoueurAParlerPostflop; }
        }

        /// <summary>
        /// Indice (correspondant à FFTabJoueurs) qui représente le joueur à qui est le tour de jouer.
        /// </summary>
        public int PIndJoueurActuel { protected set; get; }
        /// <summary>
        /// Numbers of total hands in the session
        /// </summary>
        public int PHandCount { get; set; }

        public decimal PSmallBlind { protected set; get; }
        public decimal PBigBlind { protected set; get; }
        public decimal PAntes { protected set; get; }
        public decimal PPot { protected set; get; }
        public decimal PNbJetonsDepart { protected set; get; }
        /// <summary>
        /// Dernière mise du dernier joueur ayant "bet" ou "raise" au cours d'une main.
        /// </summary>
        public decimal PDerniereMise { protected set; get; }

        public bool PContinuePlaying { protected set; get; }

        /// <summary>
        /// Stade de la main actuel (preflop, flop, turn ou river)
        /// </summary>
        public ToursPossible PStadeMain
        {
            protected set
            {
                if (!Enum.IsDefined(typeof(CTableInfos.ToursPossible), value))
                    throw new ArgumentException();

                FFStadeMain = value;
            }
            get { return FFStadeMain; }
        }

        /// <summary>
        /// Retourner le nombre de jetons pour un joueur spécifique dans une main actuel.
        /// </summary>
        /// <param name="_indJoueur">Indice du joueur que l'on souhaite retourner ses jetons.</param>
        /// <returns>Retourne le nombre de jetons du joueur correspond à _indJoueur.</returns>
        public decimal RetournerNbJetonPourUnJoueur(int _indJoueur)
        {
            if (_indJoueur < 0 || FFTabJoueurs == null || _indJoueur >= FFTabJoueurs.Length)
                throw new InvalidOperationException("L'indice de joueur reçu est invalide.");

            return FFTabJoueurs[_indJoueur].PNumberOfChipsLeft;
        }

        /// <summary>
        /// Retourner une carte.
        /// </summary>
        /// <param name="_indJoueur">Indice du joueur que l'on souhaite retourner sa carte.</param>
        /// <param name="_indCarte">Indice de la carte (1, 2 seulement possible)</param>
        /// <returns>Retourne la carte.</returns>
        public string GetOneCardFromOnePlayer(int _indJoueur, int _indCarte)
        {
            if (_indJoueur < 0 || FFTabJoueurs == null || _indJoueur >= FFTabJoueurs.Length ||
                FFTabJoueursCartes == null || FFTabJoueursCartes[_indJoueur] == "")
                throw new InvalidOperationException("L'indice de joueur reçu est invalide.");
            else if (_indCarte < 0 || _indCarte > 1)
                throw new InvalidOperationException("L'indice de la carte reçu est invalide.");

            return FFTabJoueursCartes[_indJoueur].Split(' ')[_indCarte];
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

        public CGameController(List<CPlayer> _lstPlayers, decimal _nbJetonsDepart, decimal _smallBlind, decimal _bigBlind, decimal _antes, bool _resetStacksEveryHand)
        {
            #region Local methods
            Action LFInitialiserJoueurs = () =>
            {
                // Création des joueurs
                FFTabJoueurs = _lstPlayers.ToArray();
                // Création de l'historique des actions des joueurs
                FFLstActionsMainActuel = new List<Tuple<CAction, int>>();
                FFLstActionsMainActuelParJoueur = new Dictionary<CPlayer, List<CAction>[]>();
                // Création de la liste qui indique les actions possible pour un joueur X
                FLstActionsPossibleJoueurActuel = new HashSet<ActionsPossible>();
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X
                FFStackDepartDebutMain = new Dictionary<int, decimal>();
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X au début de la game
                FFStackDepartDebutPartie = new Dictionary<int, decimal>();

                foreach (CPlayer player in _lstPlayers)
                {
                    FFLstActionsMainActuelParJoueur[player] = new List<CAction>[CCSTREETS_COUNT];
                    FFLstActionsMainActuelParJoueur[player][(int)ToursPossible.Preflop] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)ToursPossible.Flop] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)ToursPossible.Turn] = new List<CAction>();
                    FFLstActionsMainActuelParJoueur[player][(int)ToursPossible.River] = new List<CAction>();
                }


                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                    FFStackDepartDebutPartie[indJoueur] = FFTabJoueurs[indJoueur].PNumberOfChipsLeft;
            };
            #endregion

            if (_lstPlayers.Count <= 1)
                throw new ArgumentException("_lstPlayers must have atleast 2 players!", "_lstPlayers");

            #region Initialisation des paramètres de la partie
            PSmallBlind = _smallBlind;
            PBigBlind = _bigBlind;
            PAntes = _antes;            
            PNbJetonsDepart = _nbJetonsDepart;
            FFResetStacksEveryHand = _resetStacksEveryHand;
            PIndJoueurActuel = -1;
            PContinuePlaying = true;            

            FFIndPremierJoueurAParlerPostflop = -1;
            FFIndPremierJoueurAParlerPostflopProchaineMain = -1;
            FFIndDernierJoueurAParler = -1;
            FFBoard = "";

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
                FFLstActionsMainActuel = new List<Tuple<CAction, int>>(_controller.FFLstActionsMainActuel.Count);
                foreach (Tuple<CAction, int> currentTuple in FFLstActionsMainActuel)
                    FFLstActionsMainActuel.Add(new Tuple<CAction, int>((CAction)currentTuple.Item1.Clone(), currentTuple.Item2));

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
                FLstActionsPossibleJoueurActuel = new HashSet<ActionsPossible>(_controller.FLstActionsPossibleJoueurActuel);
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X
                FFStackDepartDebutMain = new Dictionary<int, decimal>(_controller.FFStackDepartDebutMain);
                // Création d'un dictionnaire qui indique le stack de départ d'un joueur X au début de la game
                FFStackDepartDebutPartie = new Dictionary<int, decimal>(_controller.FFStackDepartDebutPartie);
            };
            #endregion

            #region Initialisation des paramètres de la partie
            PSmallBlind = _controller.PSmallBlind;
            PBigBlind = _controller.PBigBlind;
            PAntes = _controller.PAntes;
            PNbJetonsDepart = _controller.PNbJetonsDepart;
            FFResetStacksEveryHand = _controller.FFResetStacksEveryHand;
            PIndJoueurActuel = _controller.PIndJoueurActuel;
            PContinuePlaying = _controller.PContinuePlaying;
            PStadeMain = _controller.PStadeMain;
            PPot = _controller.PPot;
            PDerniereMise = _controller.PDerniereMise;

            FFIndPremierJoueurAParlerPostflop = _controller.PIndPremierJoueurAParlerPostflop;
            FFIndPremierJoueurAParlerPostflopProchaineMain = _controller.FFIndPremierJoueurAParlerPostflopProchaineMain;
            FFIndDernierJoueurAParler = _controller.FFIndDernierJoueurAParler;
            FFBoard = String.Copy(_controller.FFBoard);

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
     //       if (_indJoueur == 0)
       //         indCarteActuel = 16;
            
            cartes = FFLstCartes.ElementAt(indCarteActuel).ToString();
            FFLstCartes.RemoveAt(indCarteActuel);

            for (int iNbCarte = 1; iNbCarte < _nbCartesATirer; iNbCarte++)
            {
                indCarteActuel = CRandomNumberHelper.Between(0, FFLstCartes.Count - 1);
        //        if (_indJoueur == 0)
          //          indCarteActuel = 20;
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
        protected CCard GetRandomCard()
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
        #region Méthodes pour les joueurs
        protected bool LastPlayerPlayed()
        {
            return !(((FFLstJoueursPasFold.Contains(FFIndDernierJoueurAParler) && PIndJoueurActuel != FFIndDernierJoueurAParler) || (!FFLstJoueursPasFold.Contains(FFIndDernierJoueurAParler) && PIndJoueurActuel != FFLstJoueursPasFold.Last())));
        }

        protected int GetNextPlayerIndex()
        {
            return CListHelper.ElemNextOf(FFLstJoueursPasFold, PIndJoueurActuel);
        }

        protected void MakeEveryoneAliveAndResetBets()
        {
            FFLstJoueursPasFold.Clear();

            // Tout le monde est vivant.
            for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
            {
                FFLstJoueursPasFold.Add(indJoueur);
                FFTabJoueurs[indJoueur].PLastBet = 0; // Ici est la réinitialisation de la mise

                if (FFTabJoueurs[indJoueur].PNumberOfChipsLeft < FFStackDepartDebutPartie[indJoueur])
                    FFTabJoueurs[indJoueur].PNumberOfChipsLeft = FFStackDepartDebutPartie[indJoueur];

                #region Sauvegarder les stacks de départ des joueurs (utile pour les split pots et les all in pots pour savoir le effective stack
                FFStackDepartDebutMain[indJoueur] = FFTabJoueurs[indJoueur].PNumberOfChipsLeft;
                FFTabJoueurs[indJoueur].PNumberOfChipsAtBeginningHand = FFTabJoueurs[indJoueur].PNumberOfChipsLeft;
                #endregion
            }
        }

        #endregion

        public virtual void PlayNewHand()
        {
            #region Local methods
            Action LFReinitialiserLstCartes = (() => {
                FFLstCartes.Clear();
                #region Création de la liste des cartes
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
            });

            Action LFInitialiserJoueurs = (() =>
            {                
                for (int indJoueur = 0; indJoueur < FFTabJoueurs.Length; ++indJoueur)
                {
                    if (FFResetStacksEveryHand)
                        FFTabJoueurs[indJoueur].PNumberOfChipsLeft = FFStackDepartDebutPartie[indJoueur];

                    #region Distribuer des cartes aux joueurs
                    DistribuerCartes(indJoueur, 2);
                    #endregion
                }
            });

            Action LFReinitialiserStadeMain = (() =>
            {
                PStadeMain = ToursPossible.Preflop;
            });
            #endregion            
            
            LFReinitialiserLstCartes();
            LFReinitialiserStadeMain();

            LFInitialiserJoueurs();        
        }
        public Task PlayNewHandAsync()
        {
            return Task.Run(() =>
            {
                PlayNewHand();
            });
        }

        public virtual void StopGame()
        {
            PContinuePlaying = false;
        }

        public Task StopGameAsync()
        {
            return Task.Run(() =>
            {
                StopGame();
            });
        }

        protected abstract void Play();

        protected Task PlayAsync()
        {
            return Task.Run(() =>
            {
                Play();
            });
        }

        protected virtual void GameStopped()
        {
            PEventGameStopped(this);
        }

        public abstract void Fold();
        public abstract void Check();
        public abstract void Call();
        public abstract void Bet(decimal _mise);
        public abstract void Raise(decimal _mise);
    }
}
