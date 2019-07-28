/*
	4 mai 2015 par Jonathan Clavet-Grenier (jonathanclavetg@gmail.com)
	Gérer l'interface pour créer une partie.
*/

using Amigo.Controllers;
using Amigo.Helpers;
using HoldemHand;
using Shared.Poker.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Amigo
{
    public partial class frmCreerPartie : Form
    {

        /* Début des constantes qui représentent les messages d'erreurs possibles. */

        const string MESSAGE_ERREUR_NBJETONPARNIVEAU = "Vous ne pouvez pas avoir un niveau supérieur avec moins de jetons obligatoire qu'un niveau inférieur.";
        const string MESSAGE_ERREUR_NBJETONMANQUANT = "Veuillez entrer une valeur pour tous les niveaux.";

        /* Fin */

        /* Début des données membres. */

        private int FFNbJoueurs; // Entier qui représente le nombre de joueurs que l'utilisateur a choisi.
        private int FFNbRobots;  // Entier qui représente le nombre de robots que l'utilisateur a choisi.

        public CGamesManagerController PJeu;    // Pointeur de type CPoker qui pointe vers le programme principal.

        /* Fin */

        /*********************************** CONSTRUCTEURS ***********************************/

        public frmCreerPartie()
        /*
            Objectif: Constructeur de la classe obligatoire. On ne devrait pas utiliser ce constructeur.
        */
        {
            // Affichage des composants visuelles de la fenêtre.
            InitializeComponent();
            // Affectation des données membres à une valeur par défaut.
            FFNbJoueurs = 2;
            FFNbRobots = 0;
            // Affecter une sélection des ComboBox par défaut.
            cmbSimulatorNbOfPlayers.SelectedIndex = 0;
            cmbScenarioManagerNbOfPlayers.SelectedIndex = 0;            
            // Mettre un focus par défaut au premier élément de la fiche.
            cmbScenarioManagerNbOfPlayers.Focus();
        }

        /*********************************** FIN ***********************************/

        /*********************************** ÉVÉNEMENTS ***********************************/

        private void frmCreerPartie_FormClosed(object sender, FormClosedEventArgs e)
        /*
            Objectif: Effacer la Form de la mémoire.
               Cause: Événement qui se produit lorsque l'utilisateur ferme la fenêtre.
        */
        {
            // On assigne le pointeur qui pointe vers nous à null puisque la Form est fermé.
            PJeu.Close();
        }

        private void cmbNbJoueur_SelectedIndexChanged(object sender, EventArgs e)
        /*
            Objectif: Changer les valeurs des données membres.
               Cause: Événement qui se produit lorsque l'utilisateur change le nombre de joueurs.          
        */
        {
            // Affecter le nouveau nombre de joueurs à la donnée membre correspondante.
            FFNbJoueurs = cmbScenarioManagerNbOfPlayers.SelectedIndex + 2;
        }

        private void txtMinuteJetonNiveau_KeyPress(object sender, KeyPressEventArgs e)
        /*
            Objectif: Empêcher l'utiliser d'entrer des chiffres dans les TextBox correspondant aux minutes, jetons et
                      niveaux.
               Cause: Se produit lorsque l'utilisateur appuie sur une touche de clavier dans ces TextBox correspondant.
        */
        {
            // Accepter seulement des caractères entre 0 et 9. De plus, accepter le caractère Backspace (8).
            e.Handled = !(e.KeyChar >= '0' && e.KeyChar <= '9' || e.KeyChar == 8);
        }


        private void chkBox_ToucheAppuye(object sender, KeyPressEventArgs e)
        /*
            Objectif: Permettre l'utilisateur de changer l'état d'une case à cocher lorsqu'il appuie sur Entrée.
               Cause: Événement qui se produit lorsque une case à cocher a le focus et que l'utilisateur appuie sur Entrée.
        */
        {
            if (e.KeyChar == 13)
                (sender as CheckBox).Checked = !(sender as CheckBox).Checked;
        }

        private void dgvRobots_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Source: http://stackoverflow.com/questions/1718389/right-click-context-menu-for-datagridview
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DataGridViewCell c = (sender as DataGridView)[e.ColumnIndex, e.RowIndex];
                if (!c.Selected)
                {
                    c.DataGridView.ClearSelection();
                    c.DataGridView.CurrentCell = c;
                    c.Selected = true;
                }
            }
        }

        private void btnSimuler_Click(object sender, EventArgs e)
        {
            btnSimuler.Text = "Arrêter";

            double antes = 0;
            double smallBlind = 0;
            double bigBlind = 0;
            double nbJetonsDepart = 0;

            CultureInfo Culture = new CultureInfo("en-CA");

            txtAntes.Text = txtAntes.Text.Replace(',', '.');
            txtSmallBlind.Text = txtSmallBlind.Text.Replace(',', '.');
            txtBigBlind.Text = txtBigBlind.Text.Replace(',', '.');
            txtNbJetonDepart.Text = txtNbJetonDepart.Text.Replace(',', '.');

            if (double.TryParse(txtSmallBlind.Text, NumberStyles.Float, Culture, out smallBlind) &&
                double.TryParse(txtBigBlind.Text, NumberStyles.Float, Culture, out bigBlind) &&
                double.TryParse(txtNbJetonDepart.Text, NumberStyles.Float, Culture, out nbJetonsDepart) &&
                nbJetonsDepart > bigBlind)
            {
                CPlayer player1 = null;
                CPlayer player2 = null;

                player1 = new CPlayer(nbJetonsDepart, "Amigo");
                player2 = new CPlayer(nbJetonsDepart, "TheTypicalFish");                

                List<CPlayer> lstPlayers = new List<CPlayer>(2) { player1, player2 };
                PJeu.SimulateGamesAsync(lstPlayers, smallBlind, bigBlind, antes, rbYes.Checked);
            }
            else
                MessageBox.Show("Vérifiez votre nombre de jetons ainsi que votre mise obligatoire.");
        }

        public void UpdateSimulationInfos(Dictionary<string, CSessionInfo> _dicPlayersSessionInfos, int _handCount)
        {            
            if (!IsDisposed)
            {
                string playerThatHasTheHighestProfitName = null;
                double highestProfit = -1;

                foreach (string playerName in _dicPlayersSessionInfos.Keys)
                {
                    if (_dicPlayersSessionInfos[playerName].PNbProfit > highestProfit)
                    {
                        playerThatHasTheHighestProfitName = playerName;
                        highestProfit = _dicPlayersSessionInfos[playerName].PNbProfit;
                    }
                }

                CSessionInfo playerSessionInfo = _dicPlayersSessionInfos[playerThatHasTheHighestProfitName];
                double bigBlind = double.Parse(txtBigBlind.Text);

                double playerStackInBB = (highestProfit / bigBlind);
                double playerBB100WinRate = Math.Round(((playerStackInBB / _handCount) * 100), 2);
                double playerNbrOfPotsWon = playerSessionInfo.PNbWins;
                double playerNbrOfPotsWonInPercentage = Math.Round(((playerNbrOfPotsWon / _handCount) * 100), 2);
                             
                if (InvokeRequired)
                {
                    lblWinningPlayer.Invoke(new Action(() => {
                        lblWinningPlayer.Text = playerThatHasTheHighestProfitName + " wins (" + playerBB100WinRate.ToString() + " BB/100)";
                        lblNbrOfProfit.Text = highestProfit.ToString() + " $";
                        lblNbrOfPotsWon.Text = playerNbrOfPotsWonInPercentage.ToString() + "%";
                        lblNbrOfHands.Text = _handCount.ToString();
                    }));                    
                }
                else
                {
                    lblWinningPlayer.Text = playerThatHasTheHighestProfitName + " wins (" + playerBB100WinRate.ToString() + " BB/100)";
                    lblNbrOfProfit.Text = highestProfit.ToString() + " $";
                    lblNbrOfPotsWon.Text = playerNbrOfPotsWonInPercentage.ToString() + "%";
                    lblNbrOfHands.Text = _handCount.ToString();
                }
            }
        }

        private void cmsPlayers_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "tsmiAddPlayer")
            {
                frmOptions frm = new frmOptions();
                frm.ShowDialog();
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnPlayHand_Click(object sender, EventArgs e)
        {
            PJeu.StartReplayerAsync(richTxtHand.Text);
        }
    }
}
