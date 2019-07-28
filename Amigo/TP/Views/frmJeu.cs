using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using Shared.Helpers;
using Amigo.Interfaces;
using Amigo.Events;
using System.Collections.Generic;
using static Shared.Poker.Models.CAction;
using System.Threading;
using Shared.Poker.Models;
using static Shared.Poker.Models.CTableInfos;
using Amigo.Helpers;
using HoldemHand;

namespace Amigo
{
    public sealed partial class frmJeu : Form
    {       
        public enum DEALER_POS { SB = 0, BB = 1, UTG = 2, MP = 3, CO = 4, BTN = 5 };
        public enum DEALER_POS_X { SB = 373, BB = 213, UTG = 320, MP = 700, CO = 834, BTN = 661 };
        public enum DEALER_POS_Y { SB = 245, BB = 230, UTG = 140, MP = 150, CO = 263, BTN = 299 };

        private int FNbJoueurs;
        private double FFBigBlind;
        private double FFPot;        
        private double FFHeroNumberOfChipsLeft;
        private double FFVillainLastBet;
        private IGameController FFController;

        private CultureInfo FLangage;
        private DEALER_POS_X FFDealerPosX;
        private DEALER_POS_Y FDealerPosY;

        #region Delegates
        private delegate void DAfficherNbJetonDunJoueur(int _playerIndex, double _chipsLeft);
        private delegate void DChangerJoueurJeton(int _indJoueur, int _iNbJeton);
        private delegate void DChangerPot(int _NbJetons);
        private delegate void DMettreAJourChoixDuJoueur(bool _peutFolder, bool _peutCheck, bool _peutCall, bool _peutBet, bool _peutRaise);
        private delegate void DAfficherFlop(string _Carte1, string _Carte2, string _Carte3);
        private delegate void DAfficherTurn(string _Carte);
        private delegate void DAfficherRiver(string _Carte);
        private delegate void DAfficherCartes(int _IndJoueur, string _Cartes);
        private delegate void DAfficherCartesCachee(int _IndJoueur);
        private delegate void DCacherCartes(int _IndJoueur);
        private delegate void DResetActions(bool _ResetBoard);
        private delegate void DAfficherGagnerPotDunJoueur(int _playerIndex, double _pot);
        private delegate void DAfficherCheckDunJoueur(int _indJoueur);
        private delegate void DAfficherCallDunJoueur(int _indJoueur, double _betSize, double _pot);
        private delegate void DAfficherFoldDunJoueur(int _indJoueur);
        private delegate void DAfficherMiseDunJoueur(int _indJoueur, double _betSize, double _pot);

        private delegate void DAfficherRaiseDunJoueur(int _indJoueur, double _raiseSize, double _pot);
        private delegate void DAfficherPot(double _pot);
        private delegate void DAfficherJoueurs(int _IndDebut, int _IndFin);
        private delegate void DChangerNomJoueur(string _newPlayerName, int _indexPlayer);
        private delegate void DAfficherSmallBlind(int _IndJoueurTour, double _smallBlind);
        private delegate void DAfficherBigBlind(int _IndJoueurTour, double _bigBlind);
        private delegate void DPositionnerDealer(DEALER_POS _dealerPosition);
        private delegate DEALER_POS_X DRetournerPosDealerX();
        private delegate DEALER_POS_Y DRetournerPosDealerY();
        private delegate void DRefresh();

        private DAfficherNbJetonDunJoueur FFDelegateAfficherNbJetonDunJoueur;
        private DChangerJoueurJeton FFDelegateChangerJoueurJeton;
        private DChangerPot FFDelegateChangerPot;
        private DMettreAJourChoixDuJoueur FFDelegateMettreAJourChoixDuJoueur;
        private DAfficherFlop FFDelegateAfficherFlop;
        private DAfficherTurn FFDelegateAfficherTurn;
        private DAfficherRiver FFDelegateAfficherRiver;
        private DAfficherCartes FFDelegateAfficherCartes;
        private DAfficherCartesCachee FFDelegateAfficherCartesCachee;
        private DCacherCartes FFDelegateCacherCartes;
        private DResetActions FFDelegateResetActions;
        private DAfficherGagnerPotDunJoueur FFDelegateAfficherGagnerPotDunJoueur;
        private DAfficherCheckDunJoueur FFDelegateAfficherCheckDunJoueur;
        private DAfficherCallDunJoueur FFDelegateAfficherCallDunJoueur;
        private DAfficherFoldDunJoueur FFDelegateAfficherFoldDunJoueur;
        private DAfficherMiseDunJoueur FFDelegateAfficherMiseDunJoueur;
        private DAfficherRaiseDunJoueur FFDelegateAfficherRaiseDunJoueur;
        private DAfficherPot FFDelegateAfficherPot;
        private DAfficherJoueurs FFDelegateAfficherJoueurs;
        private DChangerNomJoueur FFDelegateChangerNomJoueur;
        private DAfficherSmallBlind FFDelegateAfficherSmallBlind;
        private DAfficherBigBlind FFDelegateAfficherBigBlind;
        private DPositionnerDealer FFDelegatePositionnerDealer;
        private DRetournerPosDealerX FFDelegateRetournerPosDealerX;
        private DRetournerPosDealerY FFDelegateRetournerPosDealerY;
        private DRefresh FFDelegateRefresh;
        #endregion
        
        public frmJeu(IGameController _controller)
        {
            Initialize(_controller);
        }

        public frmJeu(IGameHumanVsBotController _controller)
        {
            Initialize(_controller);
            _controller.POnWaitingForHumanAction += OnWaitingForHumanAction;
        }

        private void Initialize(IGameController _controller)
        {
            FFController = _controller;

            FNbJoueurs = 0;

            FFDealerPosX = DEALER_POS_X.BTN;
            FDealerPosY = DEALER_POS_Y.BTN;
            FLangage = new CultureInfo("en-CA");

            #region Delegates
            FFDelegateAfficherNbJetonDunJoueur = AfficherNbJetonDunJoueur;
            FFDelegateChangerJoueurJeton = ChangerJoueurJeton;
            FFDelegateChangerPot = ChangerPot;
            FFDelegateMettreAJourChoixDuJoueur = MettreAJourChoixDuJoueur;
            FFDelegateAfficherFlop = AfficherFlop;
            FFDelegateAfficherTurn = AfficherTurn;
            FFDelegateAfficherRiver = AfficherRiver;
            FFDelegateAfficherCartes = AfficherCartes;
            FFDelegateAfficherCartesCachee = AfficherCartesCachee;
            FFDelegateCacherCartes = CacherCartes;
            FFDelegateResetActions = ResetActions;
            FFDelegateAfficherGagnerPotDunJoueur = AfficherGagnerPotDunJoueur;
            FFDelegateAfficherCheckDunJoueur = AfficherCheckDunJoueur;
            FFDelegateAfficherCallDunJoueur = AfficherCallDunJoueur;
            FFDelegateAfficherFoldDunJoueur = AfficherFoldDunJoueur;
            FFDelegateAfficherMiseDunJoueur = AfficherMiseDunJoueur;
            FFDelegateAfficherRaiseDunJoueur = AfficherRaiseDunJoueur;
            FFDelegateAfficherPot = AfficherPot;
            FFDelegateAfficherJoueurs = AfficherJoueurs;
            FFDelegateChangerNomJoueur = ChangerNomJoueur;
            FFDelegateAfficherSmallBlind = AfficherSmallBlind;
            FFDelegateAfficherBigBlind = AfficherBigBlind;
            FFDelegatePositionnerDealer = PositionnerDealer;
            FFDelegateRetournerPosDealerX = RetournerPosDealerX;
            FFDelegateRetournerPosDealerY = RetournerPosDealerY;
            FFDelegateRefresh = Refresh;
            #endregion

            InitializeComponent();

            _controller.POnNewHand += OnNewHand;
            _controller.POnNewAction += OnNewAction;
            _controller.POnNewStreet += OnNewStreet;
            _controller.POnHandFinished += OnHandFinished;
            picDealer.Image = CImageHelper.ScaleImage((Properties.Resources.ResourceManager.GetObject("dealer") as Image), picDealer.Width, picDealer.Height);
        }

        public void OnNewHand(object sender, COnNewHandEventArgs e)
        {
            #region Local methods
            void LFUpdatePlayersNames()
            {
                foreach (var playerInfos in e.PPlayerNames)
                {
                    if (string.IsNullOrEmpty(playerInfos.Value))
                        ChangerNomJoueur("Joueur " + playerInfos.Key.ToString(), playerInfos.Key);
                    else
                        ChangerNomJoueur(playerInfos.Value, playerInfos.Key);
                }
            }
            void LFUpdatePlayersCards()
            {
                foreach (var playerInfos in e.PPlayerCards)
                {
                    if (string.IsNullOrEmpty(playerInfos.Value))
                        AfficherCartesCachee(playerInfos.Key);
                    else
                        AfficherCartes(playerInfos.Key, playerInfos.Value);
                }

            }
            void LFUpdatePlayersChips()
            {
                foreach (var playerInfos in e.PPlayerStacks)
                    AfficherNbJetonDunJoueur(playerInfos.Key, playerInfos.Value);
            }
            void LFUpdateBTN()
            {
                PositionnerDealer((DEALER_POS)e.PBTNPlayerIndex);
            }
            #endregion

            ResetActions(true);
            LFUpdatePlayersNames();
            LFUpdatePlayersCards();
            LFUpdatePlayersChips();
            LFUpdateBTN();
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            AfficherJoueurs(1, e.PPlayerNames.Count);
            AfficherSmallBlind(e.PSmallBlind.Item1, e.PSmallBlind.Item2);
            AfficherBigBlind(e.PBigBlind.Item1, e.PBigBlind.Item2);

            Refresh();

            FFBigBlind = e.PBigBlind.Item2;
        }

        public void OnNewAction(object sender, COnNewActionEventArgs e)
        {            
            switch (e.PNewAction.PAction)
            {
                case PokerAction.Check:
                    AfficherCheckDunJoueur(e.PPlayerID);
                    break;
                case PokerAction.Bet:
                    AfficherMiseDunJoueur(e.PPlayerID, e.PNewAction.PMise, e.PPot);
                    AfficherNbJetonDunJoueur(e.PPlayerID, e.PPlayersStacks[e.PPlayerID].PNumberOfChipsLeft);
                    break;
                case PokerAction.Raise:
                    AfficherRaiseDunJoueur(e.PPlayerID, e.PNewAction.PMise, e.PPot);
                    AfficherNbJetonDunJoueur(e.PPlayerID, e.PPlayersStacks[e.PPlayerID].PNumberOfChipsLeft);
                    break;
                case PokerAction.Call:
                    AfficherCallDunJoueur(e.PPlayerID, e.PNewAction.PMise, e.PPot);
                    AfficherNbJetonDunJoueur(e.PPlayerID, e.PPlayersStacks[e.PPlayerID].PNumberOfChipsLeft);
                    break;
                case PokerAction.Fold:
                    AfficherFoldDunJoueur(e.PPlayerID);
                    break;
                default:
                    throw new InvalidOperationException("Invalid action");
            }
                        
            Refresh();

            FFPot = e.PPot;         
        }

        public void OnNewStreet(object sender, COnNewStreetEventArgs e)
        {
            switch (e.PCurrentStreet)
            {
                case Street.Flop:
                    AfficherFlop(e.PBoard.PBoardList[0].ToString(), e.PBoard.PBoardList[1].ToString(), e.PBoard.PBoardList[2].ToString());                
                    break;
                case Street.Turn:
                    AfficherTurn(e.PBoard.PBoardList[3].ToString());
                    break;
                case Street.River:
                    AfficherRiver(e.PBoard.PBoardList[4].ToString());
                    break;
                default:
                    throw new InvalidOperationException("Invalid street");
            }

            ResetActions(false);
            Refresh();            
        }

        public void OnWaitingForHumanAction(object sender, COnWaitingForHumanActionEventArgs e)
        {
            FFHeroNumberOfChipsLeft = e.PHeroNumberOfChipsLeft;
            FFVillainLastBet = e.PVillainLastBet;

            MettreAJourChoixDuJoueur(e.PHeroAllowedActions.Contains(PokerAction.Fold),
                                     e.PHeroAllowedActions.Contains(PokerAction.Check),
                                     e.PHeroAllowedActions.Contains(PokerAction.Call),
                                     e.PHeroAllowedActions.Contains(PokerAction.Bet),
                                     e.PHeroAllowedActions.Contains(PokerAction.Raise));
        }

        public void OnHandFinished(object sender, COnHandFinishedEventArgs e)
        {
            foreach (var playerInfos in e.PPlayerCards)
                AfficherCartes(playerInfos.Key, playerInfos.Value);
            
            foreach (var playerInfos in e.PLstPlayersThatWonThePot)            
                AfficherGagnerPotDunJoueur(playerInfos.Item1, playerInfos.Item2);                            

            Refresh();
            Thread.Sleep(1500);
        }

        private void txtNbJeton_KeyPress(object sender, KeyPressEventArgs e)
        /*
            Évènement qui se déclenche lorsque l'utilisateur tape le nombre de jetons qu'il veut. On lui permet d'entrer que des chiffres.
        */
        {
            e.Handled = !(e.KeyChar == 8 || (e.KeyChar == ',' && !txtNbJetons.Text.Contains('.')) || (e.KeyChar == '.' && !txtNbJetons.Text.Contains('.')) || (e.KeyChar >= '0' && e.KeyChar <= '9'));
                
            if (e.Handled)
            {
                txtNbJetons.Text = txtNbJetons.Text.Replace(',', '.');
            }
        }

        private void ChangerJoueurJeton(int _indJoueur, int _iNbJeton)
        /*
            Objectif: Changer le nombre de jetons apparant pour un joueur spécifique.
        */
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _indJoueur, _iNbJeton };

                    Invoke(FFDelegateChangerJoueurJeton, methodParameters);
                }
                else
                    // Changer le label correspondant au nombre de jetons d'un joueur spécifique.
                    ((Label)Controls.Find("Joueur" + Convert.ToString(_indJoueur + 1), true)[0]).Text = Convert.ToString(_iNbJeton);
            }
        }

        private void ChangerPot(int _NbJetons)
        /*
            Objectif: Changer le pot.
        */
        {
                if (!IsDisposed)
                {
                    // If we are NOT on the UI thread
                    if (InvokeRequired)
                    {
                        object[] methodParameters = new object[1] { _NbJetons };

                        Invoke(FFDelegateChangerPot, methodParameters);
                    }
                    else
                        txtNbJetons.Text = Convert.ToString(_NbJetons);
                }
        }

        public void MettreAJourChoixDuJoueur(bool _peutFolder, bool _peutCheck, bool _peutCall, bool _peutBet, bool _peutRaise)
        /*
            Objectif: Selon la phase qu'on est rendu, permettre au joueur de faire son choix.
        */
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[5] { _peutFolder, _peutCheck, _peutCall, _peutBet, _peutRaise };

                    Invoke(FFDelegateMettreAJourChoixDuJoueur, methodParameters);
                }
                else
                {
                    btnFold.Enabled = btnFold.Visible = _peutFolder;
                    btnCheck.Enabled = btnCheck.Visible = _peutCheck;
                    btnCall.Enabled = btnCall.Visible = _peutCall;


                    btnBet.Enabled = btnBet.Visible = btnHalfPot.Enabled = btnHalfPot.Visible = btnTwoThirdPot.Enabled =
                    btnTwoThirdPot.Visible = btnPot.Enabled = btnPot.Visible = btnMax.Enabled = btnMax.Visible =
                    txtNbJetons.Enabled = txtNbJetons.Visible = _peutBet;

                    if (_peutRaise)
                    {
                        txtNbJetons.Enabled = txtNbJetons.Visible = btnRaise.Enabled = btnRaise.Visible = btnMax.Enabled = btnMax.Visible = true;
                        txtNbJetons.Text = Convert.ToString(Math.Round(FFVillainLastBet + FFBigBlind, 2).ToString("#0.00", FLangage));
                    }
                    else
                        btnRaise.Enabled = btnRaise.Visible = false;
                }
            }
        }

        private void btnCall_Click(object sender, EventArgs e)
        /*
            Évènement qui se déclenche lorsqu'on appuie sur le bouton CALL. Envoyer le choix au serveur.
        */
        {
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            btnCall.Focus();
            Task.Run(() => { FFController.Call(); });           
        }

        private void btnRaise_Click(object sender, EventArgs e)
        /*
           Évènement qui se déclenche lorsqu'on appuie sur le bouton RAISE. Envoyer le choix au serveur.
       */
        {
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            btnRaise.Focus();
            Task.Run(() => { FFController.Raise(double.Parse(txtNbJetons.Text, FLangage)); });
        }

        private void btnFold_Click(object sender, EventArgs e)
        /*
            Évènement qui se déclenche lorsqu'on appuie sur le bouton FOLD. Envoyer le choix au serveur.
        */
        {
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            btnFold.Focus();
            Task.Run(() => { FFController.Fold(); });
        }

        private void btnCheck_Click(object sender, EventArgs e)
        /*
            Évènement qui se déclenche lorsqu'on appuie sur le bouton CHECK. Envoyer le choix au serveur.
        */
        {
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            btnCheck.Focus();
            Task.Run(() => { FFController.Check(); });
        }

        public void AfficherFlop(string _Carte1, string _Carte2, string _Carte3)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[3] { _Carte1, _Carte2, _Carte3 };

                    Invoke(FFDelegateAfficherFlop, methodParameters);
                }
                else
                {
                    PictureBox CarteActuel = null;
                    Image ImgAInserer = null;

                    picBoardCarte1.Enabled = picBoardCarte1.Visible = true;
                    picBoardCarte2.Enabled = picBoardCarte2.Visible = true;
                    picBoardCarte3.Enabled = picBoardCarte3.Visible = true;

                    for (int IndCarte = 1; IndCarte <= 3; IndCarte++)
                    {
                        CarteActuel = (Controls.Find("picBoardCarte" + Convert.ToString(IndCarte), false)[0] as PictureBox);

                        switch (IndCarte)
                        {
                            case 1:
                                ImgAInserer = (Properties.Resources.ResourceManager.GetObject("_" + _Carte1) as Image);
                                break;
                            case 2:
                                ImgAInserer = (Properties.Resources.ResourceManager.GetObject("_" + _Carte2) as Image);
                                break;
                            case 3:
                                ImgAInserer = (Properties.Resources.ResourceManager.GetObject("_" + _Carte3) as Image);
                                break;
                        }

                        CarteActuel.Image = CImageHelper.ScaleImage(ImgAInserer, CarteActuel.Width, CarteActuel.Height);
                    }

                    CarteActuel = null;
                    ImgAInserer = null;
                }
            }
        }

        public void AfficherTurn(string _Carte)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _Carte };

                    Invoke(FFDelegateAfficherTurn, methodParameters);
                }
                else
                {
                    Image ImgAInserer = (Properties.Resources.ResourceManager.GetObject("_" + _Carte) as Image);

                    picBoardCarte4.Enabled = picBoardCarte4.Visible = true;
                    picBoardCarte4.Image = CImageHelper.ScaleImage(ImgAInserer, picBoardCarte4.Width, picBoardCarte5.Height);
                }
            }
        }

        public void AfficherRiver(string _Carte)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _Carte };

                    Invoke(FFDelegateAfficherRiver, methodParameters);
                }
                else
                {
                    Image ImgAInserer = (Properties.Resources.ResourceManager.GetObject("_" + _Carte) as Image);

                    picBoardCarte5.Enabled = picBoardCarte5.Visible = true;
                    picBoardCarte5.Image = CImageHelper.ScaleImage(ImgAInserer, picBoardCarte5.Width, picBoardCarte5.Height);
                }
            }
        }

        public void AfficherCartes(int _IndJoueur, string _Cartes)
        /*
            Objectif: Afficher les cartes selon les cartes reçu en paramètre.
        */
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _IndJoueur, _Cartes };

                    Invoke(FFDelegateAfficherCartes, methodParameters);
                }
                else
                {
                    PictureBox CarteActuel1 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '1', false)[0] as PictureBox);
                    PictureBox CarteActuel2 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '2', false)[0] as PictureBox);

                    Image ImgAInserer1 = (Properties.Resources.ResourceManager.GetObject("_" + _Cartes.Split(' ')[0]) as Image);
                    Image ImgAInserer2 = (Properties.Resources.ResourceManager.GetObject("_" + _Cartes.Split(' ')[1]) as Image);

                    CarteActuel1.Image = CImageHelper.ScaleImage(ImgAInserer1, CarteActuel1.Width, CarteActuel1.Height);
                    CarteActuel1.Enabled = CarteActuel1.Visible = true;

                    CarteActuel2.Image = CImageHelper.ScaleImage(ImgAInserer2, CarteActuel2.Width, CarteActuel2.Height);
                    CarteActuel2.Enabled = CarteActuel2.Visible = true;
                }
            }
        }

        public void AfficherCartesCachee(int _IndJoueur)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _IndJoueur };

                    Invoke(FFDelegateAfficherCartesCachee, methodParameters);
                }
                else
                {
                    PictureBox carteActuel1 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '1', false)[0] as PictureBox);
                    PictureBox carteActuel2 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '2', false)[0] as PictureBox);

                    Image imgAInserer = (Properties.Resources.ResourceManager.GetObject("back2") as Image);

                    carteActuel1.Enabled = carteActuel1.Visible = true;
                    carteActuel2.Enabled = carteActuel2.Visible = true;

                    carteActuel1.Image = CImageHelper.ScaleImage(imgAInserer, carteActuel1.Width, carteActuel1.Height);
                    carteActuel2.Image = CImageHelper.ScaleImage(imgAInserer, carteActuel2.Width, carteActuel2.Height);
                }
            }
        }

        public void CacherCartes(int _IndJoueur)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _IndJoueur };

                    Invoke(FFDelegateCacherCartes, methodParameters);
                }
                else
                {
                    PictureBox CarteActuel1 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '1', false)[0] as PictureBox);
                    PictureBox CarteActuel2 = (Controls.Find("picCarte" + Convert.ToString(_IndJoueur + 1) + '2', false)[0] as PictureBox);

                    CarteActuel1.Enabled = CarteActuel1.Visible = false;
                    CarteActuel2.Enabled = CarteActuel2.Visible = false;
                }
            }
        }

        public void ResetActions(bool _ResetBoard)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _ResetBoard };

                    Invoke(FFDelegateResetActions, methodParameters);
                }
                else
                {
                    for (int IndLabel = 1; IndLabel <= 6; IndLabel++)
                        (Controls.Find("lblChoix" + Convert.ToString(IndLabel), false)[0] as Label).ResetText();

                    if (_ResetBoard)
                    {
                        picBoardCarte1.Enabled = picBoardCarte1.Visible = false;
                        picBoardCarte2.Enabled = picBoardCarte2.Visible = false;
                        picBoardCarte3.Enabled = picBoardCarte3.Visible = false;
                        picBoardCarte4.Enabled = picBoardCarte4.Visible = false;
                        picBoardCarte5.Enabled = picBoardCarte5.Visible = false;

                        picBoardCarte1.Image = null;
                        picBoardCarte2.Image = null;
                        picBoardCarte3.Image = null;
                        picBoardCarte4.Image = null;
                        picBoardCarte5.Image = null;
                    }
                }
            }
        }

        public void AfficherGagnerPotDunJoueur(int _playerIndex, double _pot)
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _playerIndex, _pot };

                    Invoke(FFDelegateAfficherGagnerPotDunJoueur, methodParameters);
                }
                else
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_playerIndex + 1)), true)[0]).Text = "A REMPORTÉ " + Convert.ToString(Math.Round(_pot, 2)) + "$";
            }
        }

        public void AfficherCheckDunJoueur(int _indJoueur)
        /*
            Objectif: Afficher qu'un joueur a check.
        */
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _indJoueur };

                    Invoke(FFDelegateAfficherCheckDunJoueur, methodParameters);
                }
                else
                {
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_indJoueur + 1)), true)[0]).Text = "CHECK";
                }                    
            }
        }

        public void AfficherCallDunJoueur(int _indJoueur, double _betSize, double _pot)
        /*
            Objectif: Afficher qu'un joueur a call.
        */
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[3] { _indJoueur, _betSize, _pot };

                    Invoke(FFDelegateAfficherCallDunJoueur, methodParameters);
                }
                else
                {
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_indJoueur + 1)), true)[0]).Text = "CALL " + Math.Round(_betSize, 2).ToString("#0.00", FLangage) + "$";
                    lblPot.Text = "Pot: " + Math.Round(_pot, 2).ToString("#0.00", FLangage) + '$';
                }
            }
        }

        public void AfficherFoldDunJoueur(int _indJoueur)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _indJoueur };

                    Invoke(FFDelegateAfficherFoldDunJoueur, methodParameters);
                }
                else
                {
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_indJoueur + 1)), true)[0]).Text = "FOLD";                    
                    CacherCartes(_indJoueur);
                }
            }
        }

        public void AfficherMiseDunJoueur(int _indJoueur, double _betSize, double _pot)
        /*
            Objectif: Afficher qu'un joueur a misé.
        */
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[3] { _indJoueur, _betSize, _pot };

                    Invoke(FFDelegateAfficherMiseDunJoueur, methodParameters);
                }
                else
                {
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_indJoueur + 1)), true)[0]).Text = "MISE " + Math.Round(_betSize, 2).ToString("#0.00", FLangage) + "$";
                    lblPot.Text = "Pot: " + Math.Round(_pot, 2).ToString("#0.00", FLangage) + '$';
                }
            }
        }

        public void AfficherRaiseDunJoueur(int _indJoueur, double _raiseSize, double _pot)
        /*
            Objectif: Afficher qu'un joueur a raise.
        */
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[3] { _indJoueur, _raiseSize, _pot };

                    Invoke(FFDelegateAfficherRaiseDunJoueur, methodParameters);
                }
                else
                {
                    ((Label)Controls.Find("lblChoix" + Convert.ToString((_indJoueur + 1)), true)[0]).Text = "RAISE " + Math.Round(_raiseSize, 2).ToString("#0.00", FLangage) + "$";
                    lblPot.Text = "Pot: " + Math.Round(_pot, 2).ToString("#0.00", FLangage) + '$';
                }
            }
        }

        public void AfficherPot(double _pot)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[1] { _pot };

                    Invoke(FFDelegateAfficherPot, methodParameters);
                }
                else
                    lblPot.Text = "Pot: " + (Math.Round(_pot, 2)).ToString("#0.00", FLangage) + '$';
            }
        }

        public void AfficherJoueurs(int _IndDebut, int _IndFin)
        /*
            Objectif: Afficher les joueurs dans la partie.
        */
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _IndDebut, _IndFin };

                    Invoke(FFDelegateAfficherJoueurs, methodParameters);
                }
                else
                {
                    int IndActuel = 1;

                    FNbJoueurs = (_IndFin - _IndDebut) + 1;

                    while (IndActuel <= 6)
                    {
                        Controls.Find("lblJoueur" + IndActuel, true)[0].Enabled = (IndActuel >= _IndDebut && IndActuel <= _IndFin);
                        Controls.Find("lblJoueur" + IndActuel, true)[0].Visible = (IndActuel >= _IndDebut && IndActuel <= _IndFin);

                        Controls.Find("lblJoueur" + IndActuel + "jeton", true)[0].Enabled = (IndActuel >= _IndDebut && IndActuel <= _IndFin);
                        Controls.Find("lblJoueur" + IndActuel + "jeton", true)[0].Visible = (IndActuel >= _IndDebut && IndActuel <= _IndFin);

                        Controls.Find("lblChoix" + IndActuel, true)[0].Enabled = (IndActuel >= _IndDebut && IndActuel <= _IndFin);
                        Controls.Find("lblChoix" + IndActuel, true)[0].Visible = (IndActuel >= _IndDebut && IndActuel <= _IndFin);

                        Controls.Find("picNoir" + IndActuel, true)[0].Enabled = (IndActuel >= _IndDebut && IndActuel <= _IndFin);
                        Controls.Find("picNoir" + IndActuel, true)[0].Visible = (IndActuel >= _IndDebut && IndActuel <= _IndFin);
                        IndActuel++;
                    }
                }
            }
        }

        /// <summary>
        /// Change the name of a specific player. Index starts at 1.
        /// </summary>
        /// <param name="_newPlayerName">New name for the player.</param>
        /// <param name="_indexPlayer">Index of the player. Index must start at 1.</param>
        public void ChangerNomJoueur(string _newPlayerName, int _indexPlayer)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _newPlayerName, _indexPlayer };

                    Invoke(FFDelegateChangerNomJoueur, methodParameters);
                }
                else
                    Controls.Find("lblJoueur" + (_indexPlayer + 1), true)[0].Text = _newPlayerName;
            }
        }

        public void AfficherNbJetonDunJoueur(int _playerIndex, double _chipsLeft) 
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _playerIndex, _chipsLeft };

                    Invoke(FFDelegateAfficherNbJetonDunJoueur, methodParameters);
                }
                else
                {
                    double nbJetons = Math.Round(_chipsLeft, 2);

                    if (nbJetons > 0)
                        ((Label)Controls.Find("lblJoueur" + Convert.ToString(_playerIndex + 1) + "Jeton", true)[0]).Text = nbJetons.ToString("#.00", FLangage) + "$";
                    else
                        ((Label)Controls.Find("lblJoueur" + Convert.ToString(_playerIndex + 1) + "Jeton", true)[0]).Text = "ALL IN";
                }
            }
        }

        private void btnBet_Click(object sender, EventArgs e) 
        /*
            Évènement qui se produit lorsqu'on clic sur le bouton pour miser.
        */
        {
            MettreAJourChoixDuJoueur(false, false, false, false, false);
            FFController.Bet(Math.Round(double.Parse(txtNbJetons.Text, FLangage), 2));
        }

        private void btnHalfPot_Click(object sender, EventArgs e) 
        /*
            Évènement qui se produit lorsqu'on clic sur le bouton "1/2 pot".
        */
        {
            double halfPotBetSize = Math.Round((FFPot * 0.5d), 2);

            if (FFHeroNumberOfChipsLeft >= halfPotBetSize)
                txtNbJetons.Text = halfPotBetSize.ToString("#0.00", FLangage);
        }

        private void btnTwoThirdPot_Click(object sender, EventArgs e)
        /*
            Évènement qui se produit lorsqu'on clic sur le bouton "2/3 pot".
        */
        {
            double TwoThirdPotBetSize = Math.Round((FFPot * 0.66d), 2);

            if (FFHeroNumberOfChipsLeft >= TwoThirdPotBetSize)
                txtNbJetons.Text = TwoThirdPotBetSize.ToString("#0.00", FLangage);
        }

        private void btnPot_Click(object sender, EventArgs e)
        /*
            Évènement qui se produit lorsqu'on clic sur le bouton "pot".
        */
        {
            double potBetSize = Math.Round(FFPot, 2);

            if (FFHeroNumberOfChipsLeft >= potBetSize)
                txtNbJetons.Text = potBetSize.ToString("#0.00", FLangage);
        }

        private void btnMax_Click(object sender, EventArgs e) {
            /*
                Évènement qui se produit lorsqu'on clic sur le bouton "max".
            */
            txtNbJetons.Text = FFPot.ToString("#0.00", FLangage);
        }

        private void txtNbJeton_Leave(object sender, EventArgs e) {
        /*
            Évènement qui se produit lorsque le focus sort de la boîte de texte correspondante aux jetons.
        */
            double NbJetons = 0;
            /*
            if (double.TryParse(txtNbJetons.Text, out NbJetons))
            {
                if (NbJetons < (FFController.PDerniereMise + FFController.PBigBlind))
                    txtNbJetons.Text = Convert.ToString(Math.Round(FFController.PDerniereMise + FFController.PBigBlind, 2));
                else if (NbJetons > (FFController.RetournerNbJetonPourUnJoueur(FFController.PIndJoueurActuel)))
                    txtNbJetons.Text = Convert.ToString(Math.Round(FFController.PDerniereMise + FFController.PBigBlind, 2));
            }
            else
                txtNbJetons.Text = Convert.ToString(Math.Round(FFController.PDerniereMise + FFController.PBigBlind, 2));*/
        }

        private void btnHalfPot_EnabledChanged(object sender, EventArgs e) {
        /*
            Évènement qui se produit lorsque l'état des boutons "1/2, 2/3, pot et max" changent.
        */
            (sender as Button).Visible = txtNbJetons.Visible = (sender as Button).Enabled;
        }

        public void AfficherSmallBlind(int _IndJoueurTour, double _smallBlind)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _IndJoueurTour, _smallBlind };

                    Invoke(FFDelegateAfficherSmallBlind, methodParameters);
                }
                else
                    (Controls.Find("lblchoix" + (_IndJoueurTour + 1), false)[0] as Label).Text = "SMALL BLIND: " + Math.Round(_smallBlind, 2).ToString("#0.00", FLangage) + '$';
            }
        }

        public void AfficherBigBlind(int _IndJoueurTour, double _bigBlind)
        {
                if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                {
                    object[] methodParameters = new object[2] { _IndJoueurTour, _bigBlind };

                    Invoke(FFDelegateAfficherBigBlind, methodParameters);
                }
                else
                    (Controls.Find("lblchoix" + (_IndJoueurTour + 1), false)[0] as Label).Text = "BIG BLIND: " + Math.Round(_bigBlind, 2).ToString("#0.00", FLangage) + '$';
            }
        }

        public void PositionnerDealer(DEALER_POS _dealerPosition)
        {
                if (!IsDisposed)
                {
                    // If we are NOT on the UI thread
                    if (InvokeRequired)
                    {
                        object[] methodParameters = new object[1] { _dealerPosition };

                        Invoke(FFDelegatePositionnerDealer, methodParameters);
                    }
                    else
                    {
                        switch (_dealerPosition)
                        {
                            case DEALER_POS.SB:
                                FFDealerPosX = DEALER_POS_X.SB;
                                FDealerPosY = DEALER_POS_Y.SB;
                                break;
                            case DEALER_POS.BB:
                                FFDealerPosX = DEALER_POS_X.BB;
                                FDealerPosY = DEALER_POS_Y.BB;
                                break;
                            case DEALER_POS.UTG:
                                FFDealerPosX = DEALER_POS_X.UTG;
                                FDealerPosY = DEALER_POS_Y.UTG;
                                break;
                            case DEALER_POS.MP:
                                FFDealerPosX = DEALER_POS_X.MP;
                                FDealerPosY = DEALER_POS_Y.MP;
                                break;
                            case DEALER_POS.CO:
                                FFDealerPosX = DEALER_POS_X.CO;
                                FDealerPosY = DEALER_POS_Y.CO;
                                break;
                            case DEALER_POS.BTN:
                                FFDealerPosX = DEALER_POS_X.BTN;
                                FDealerPosY = DEALER_POS_Y.BTN;
                                break;
                        }

                        if (FNbJoueurs == 2 && _dealerPosition == DEALER_POS.BB)
                        {
                            FFDealerPosX = FFDealerPosX + 20;
                            FDealerPosY = FDealerPosY + 30;
                        }

                        picDealer.Location = new Point((int)FFDealerPosX, (int)FDealerPosY);
                    }
                }
        }

        public DEALER_POS_X RetournerPosDealerX()
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                    return (DEALER_POS_X)Invoke(FFDelegateRetournerPosDealerX);
                else
                    return (DEALER_POS_X)picDealer.Location.X;
            }
            else
                return DEALER_POS_X.BB; // Returns something random. It doesn't change anything. This situation happens when the form is closing.            
        }

        public DEALER_POS_Y RetournerPosDealerY()
        {
            if (!IsDisposed)
            {
                // If we are NOT on the UI thread
                if (InvokeRequired)
                    return (DEALER_POS_Y)Invoke(FFDelegateRetournerPosDealerY);
                else
                    return (DEALER_POS_Y)picDealer.Location.Y;
            }
            else
                return DEALER_POS_Y.BB; // Returns something random. It doesn't change anything. This situation happens when the form is closing.            
        }

        public override void Refresh()
        {
            if (!IsDisposed)
            {
                if (InvokeRequired)
                    Invoke(FFDelegateRefresh);
                else
                    base.Refresh();
            }
        }

        private void frmJeu_FormClosing(object sender, FormClosingEventArgs e)
        {
            FFController.POnNewHand -= OnNewHand;
            FFController.POnNewAction -= OnNewAction;
            FFController.POnNewStreet -= OnNewStreet;
            FFController.POnHandFinished -= OnHandFinished;            

            base.OnClosing(e);
        }
    }
}
