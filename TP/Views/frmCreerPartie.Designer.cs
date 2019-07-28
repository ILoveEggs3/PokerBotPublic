namespace Amigo {
    partial class frmCreerPartie {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grpResult = new System.Windows.Forms.GroupBox();
            this.lblNbrOfPotsWon = new System.Windows.Forms.Label();
            this.lblNbrOfProfit = new System.Windows.Forms.Label();
            this.lblNbrOfHands = new System.Windows.Forms.Label();
            this.lblWinningPlayer = new System.Windows.Forms.Label();
            this.lblNbrOfPotsWonHeader = new System.Windows.Forms.Label();
            this.lblNbrOfProfitHeader = new System.Windows.Forms.Label();
            this.lblNbrOfHandsHeader = new System.Windows.Forms.Label();
            this.btnSimuler = new System.Windows.Forms.Button();
            this.grpSimulation = new System.Windows.Forms.GroupBox();
            this.lblShowInterface = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dvtcJoueur1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dvtcJoueur2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dvtcStartingChips = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dvtcResetEverytime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmsPlayers = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAddPlayer = new System.Windows.Forms.ToolStripMenuItem();
            this.rbNo = new System.Windows.Forms.RadioButton();
            this.rbYes = new System.Windows.Forms.RadioButton();
            this.cmbSimulatorNbOfPlayers = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabSimulator = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabScenariosManager = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.lblScenarioName = new System.Windows.Forms.Label();
            this.lblNbJetonDepart = new System.Windows.Forms.Label();
            this.txtNbJetonDepart = new System.Windows.Forms.TextBox();
            this.cmbScenarioManagerNbOfPlayers = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblNbJoueur = new System.Windows.Forms.Label();
            this.lblAntes = new System.Windows.Forms.Label();
            this.dgvRobots = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsRobot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRobotType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtBigBlind = new System.Windows.Forms.TextBox();
            this.txtSmallBlind = new System.Windows.Forms.TextBox();
            this.txtAntes = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.grpResult.SuspendLayout();
            this.grpSimulation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.cmsPlayers.SuspendLayout();
            this.tabSimulator.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabScenariosManager.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobots)).BeginInit();
            this.SuspendLayout();
            // 
            // grpResult
            // 
            this.grpResult.Controls.Add(this.lblNbrOfPotsWon);
            this.grpResult.Controls.Add(this.lblNbrOfProfit);
            this.grpResult.Controls.Add(this.lblNbrOfHands);
            this.grpResult.Controls.Add(this.lblWinningPlayer);
            this.grpResult.Controls.Add(this.lblNbrOfPotsWonHeader);
            this.grpResult.Controls.Add(this.lblNbrOfProfitHeader);
            this.grpResult.Controls.Add(this.lblNbrOfHandsHeader);
            this.grpResult.Location = new System.Drawing.Point(9, 134);
            this.grpResult.Name = "grpResult";
            this.grpResult.Size = new System.Drawing.Size(213, 91);
            this.grpResult.TabIndex = 25;
            this.grpResult.TabStop = false;
            this.grpResult.Text = "Résultat";
            // 
            // lblNbrOfPotsWon
            // 
            this.lblNbrOfPotsWon.AutoSize = true;
            this.lblNbrOfPotsWon.Location = new System.Drawing.Point(146, 67);
            this.lblNbrOfPotsWon.Name = "lblNbrOfPotsWon";
            this.lblNbrOfPotsWon.Size = new System.Drawing.Size(27, 13);
            this.lblNbrOfPotsWon.TabIndex = 11;
            this.lblNbrOfPotsWon.Text = "N/A";
            // 
            // lblNbrOfProfit
            // 
            this.lblNbrOfProfit.AutoSize = true;
            this.lblNbrOfProfit.Location = new System.Drawing.Point(146, 51);
            this.lblNbrOfProfit.Name = "lblNbrOfProfit";
            this.lblNbrOfProfit.Size = new System.Drawing.Size(27, 13);
            this.lblNbrOfProfit.TabIndex = 10;
            this.lblNbrOfProfit.Text = "N/A";
            // 
            // lblNbrOfHands
            // 
            this.lblNbrOfHands.AutoSize = true;
            this.lblNbrOfHands.Location = new System.Drawing.Point(146, 36);
            this.lblNbrOfHands.Name = "lblNbrOfHands";
            this.lblNbrOfHands.Size = new System.Drawing.Size(27, 13);
            this.lblNbrOfHands.TabIndex = 9;
            this.lblNbrOfHands.Text = "N/A";
            // 
            // lblWinningPlayer
            // 
            this.lblWinningPlayer.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblWinningPlayer.ForeColor = System.Drawing.Color.Green;
            this.lblWinningPlayer.Location = new System.Drawing.Point(6, 16);
            this.lblWinningPlayer.Name = "lblWinningPlayer";
            this.lblWinningPlayer.Size = new System.Drawing.Size(207, 15);
            this.lblWinningPlayer.TabIndex = 8;
            this.lblWinningPlayer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblNbrOfPotsWonHeader
            // 
            this.lblNbrOfPotsWonHeader.AutoSize = true;
            this.lblNbrOfPotsWonHeader.Location = new System.Drawing.Point(6, 67);
            this.lblNbrOfPotsWonHeader.Name = "lblNbrOfPotsWonHeader";
            this.lblNbrOfPotsWonHeader.Size = new System.Drawing.Size(120, 13);
            this.lblNbrOfPotsWonHeader.TabIndex = 7;
            this.lblNbrOfPotsWonHeader.Text = "Nombre de pots gagnés";
            this.lblNbrOfPotsWonHeader.Click += new System.EventHandler(this.label5_Click);
            // 
            // lblNbrOfProfitHeader
            // 
            this.lblNbrOfProfitHeader.AutoSize = true;
            this.lblNbrOfProfitHeader.Location = new System.Drawing.Point(6, 51);
            this.lblNbrOfProfitHeader.Name = "lblNbrOfProfitHeader";
            this.lblNbrOfProfitHeader.Size = new System.Drawing.Size(31, 13);
            this.lblNbrOfProfitHeader.TabIndex = 6;
            this.lblNbrOfProfitHeader.Text = "Profit";
            // 
            // lblNbrOfHandsHeader
            // 
            this.lblNbrOfHandsHeader.AutoSize = true;
            this.lblNbrOfHandsHeader.Location = new System.Drawing.Point(6, 36);
            this.lblNbrOfHandsHeader.Name = "lblNbrOfHandsHeader";
            this.lblNbrOfHandsHeader.Size = new System.Drawing.Size(89, 13);
            this.lblNbrOfHandsHeader.TabIndex = 5;
            this.lblNbrOfHandsHeader.Text = "Nombre de mains";
            // 
            // btnSimuler
            // 
            this.btnSimuler.Location = new System.Drawing.Point(120, 98);
            this.btnSimuler.Name = "btnSimuler";
            this.btnSimuler.Size = new System.Drawing.Size(102, 30);
            this.btnSimuler.TabIndex = 4;
            this.btnSimuler.Text = "Simuler";
            this.btnSimuler.UseVisualStyleBackColor = true;
            this.btnSimuler.Click += new System.EventHandler(this.btnSimuler_Click);
            // 
            // grpSimulation
            // 
            this.grpSimulation.Controls.Add(this.lblShowInterface);
            this.grpSimulation.Controls.Add(this.dataGridView1);
            this.grpSimulation.Controls.Add(this.rbNo);
            this.grpSimulation.Controls.Add(this.rbYes);
            this.grpSimulation.Controls.Add(this.cmbSimulatorNbOfPlayers);
            this.grpSimulation.Controls.Add(this.grpResult);
            this.grpSimulation.Controls.Add(this.label4);
            this.grpSimulation.Controls.Add(this.btnSimuler);
            this.grpSimulation.Location = new System.Drawing.Point(6, 6);
            this.grpSimulation.Name = "grpSimulation";
            this.grpSimulation.Size = new System.Drawing.Size(965, 231);
            this.grpSimulation.TabIndex = 27;
            this.grpSimulation.TabStop = false;
            this.grpSimulation.Text = "Simulation";
            // 
            // lblShowInterface
            // 
            this.lblShowInterface.AutoSize = true;
            this.lblShowInterface.Location = new System.Drawing.Point(6, 71);
            this.lblShowInterface.Name = "lblShowInterface";
            this.lblShowInterface.Size = new System.Drawing.Size(108, 13);
            this.lblShowInterface.TabIndex = 33;
            this.lblShowInterface.Text = "Afficher une interface";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dvtcJoueur1,
            this.dvtcJoueur2,
            this.dvtcStartingChips,
            this.dvtcResetEverytime,
            this.dataGridViewTextBoxColumn4});
            this.dataGridView1.ContextMenuStrip = this.cmsPlayers;
            this.dataGridView1.Location = new System.Drawing.Point(228, 31);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.Size = new System.Drawing.Size(725, 194);
            this.dataGridView1.TabIndex = 28;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn1.HeaderText = "Nom";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            this.dataGridViewTextBoxColumn1.Width = 54;
            // 
            // dvtcJoueur1
            // 
            this.dvtcJoueur1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dvtcJoueur1.HeaderText = "Joueur 1";
            this.dvtcJoueur1.Name = "dvtcJoueur1";
            this.dvtcJoueur1.ReadOnly = true;
            this.dvtcJoueur1.Width = 68;
            // 
            // dvtcJoueur2
            // 
            this.dvtcJoueur2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.dvtcJoueur2.HeaderText = "Joueur 2";
            this.dvtcJoueur2.Name = "dvtcJoueur2";
            this.dvtcJoueur2.ReadOnly = true;
            this.dvtcJoueur2.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dvtcJoueur2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.dvtcJoueur2.Width = 49;
            // 
            // dvtcStartingChips
            // 
            this.dvtcStartingChips.HeaderText = "Nb jetons départ";
            this.dvtcStartingChips.Name = "dvtcStartingChips";
            this.dvtcStartingChips.ReadOnly = true;
            // 
            // dvtcResetEverytime
            // 
            this.dvtcResetEverytime.HeaderText = "Reset every hand";
            this.dvtcResetEverytime.Name = "dvtcResetEverytime";
            this.dvtcResetEverytime.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.HeaderText = "Description";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // cmsPlayers
            // 
            this.cmsPlayers.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddPlayer});
            this.cmsPlayers.Name = "cmsPlayers";
            this.cmsPlayers.Size = new System.Drawing.Size(168, 26);
            this.cmsPlayers.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsPlayers_ItemClicked);
            // 
            // tsmiAddPlayer
            // 
            this.tsmiAddPlayer.Name = "tsmiAddPlayer";
            this.tsmiAddPlayer.Size = new System.Drawing.Size(167, 22);
            this.tsmiAddPlayer.Text = "Ajouter un joueur";
            // 
            // rbNo
            // 
            this.rbNo.AutoSize = true;
            this.rbNo.Checked = true;
            this.rbNo.Location = new System.Drawing.Point(167, 69);
            this.rbNo.Name = "rbNo";
            this.rbNo.Size = new System.Drawing.Size(45, 17);
            this.rbNo.TabIndex = 32;
            this.rbNo.TabStop = true;
            this.rbNo.Text = "Non";
            this.rbNo.UseVisualStyleBackColor = true;
            // 
            // rbYes
            // 
            this.rbYes.AutoSize = true;
            this.rbYes.Location = new System.Drawing.Point(120, 69);
            this.rbYes.Name = "rbYes";
            this.rbYes.Size = new System.Drawing.Size(41, 17);
            this.rbYes.TabIndex = 31;
            this.rbYes.Text = "Oui";
            this.rbYes.UseVisualStyleBackColor = true;
            // 
            // cmbSimulatorNbOfPlayers
            // 
            this.cmbSimulatorNbOfPlayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSimulatorNbOfPlayers.FormattingEnabled = true;
            this.cmbSimulatorNbOfPlayers.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cmbSimulatorNbOfPlayers.Location = new System.Drawing.Point(120, 31);
            this.cmbSimulatorNbOfPlayers.Name = "cmbSimulatorNbOfPlayers";
            this.cmbSimulatorNbOfPlayers.Size = new System.Drawing.Size(102, 21);
            this.cmbSimulatorNbOfPlayers.Sorted = true;
            this.cmbSimulatorNbOfPlayers.TabIndex = 29;
            this.cmbSimulatorNbOfPlayers.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "Nombre de joueurs";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // tabSimulator
            // 
            this.tabSimulator.Controls.Add(this.tabPage1);
            this.tabSimulator.Controls.Add(this.tabScenariosManager);
            this.tabSimulator.Location = new System.Drawing.Point(5, 4);
            this.tabSimulator.Name = "tabSimulator";
            this.tabSimulator.SelectedIndex = 0;
            this.tabSimulator.Size = new System.Drawing.Size(989, 269);
            this.tabSimulator.TabIndex = 30;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.grpSimulation);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(981, 243);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Simulateur";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabScenariosManager
            // 
            this.tabScenariosManager.Controls.Add(this.groupBox1);
            this.tabScenariosManager.Location = new System.Drawing.Point(4, 22);
            this.tabScenariosManager.Name = "tabScenariosManager";
            this.tabScenariosManager.Padding = new System.Windows.Forms.Padding(3);
            this.tabScenariosManager.Size = new System.Drawing.Size(981, 243);
            this.tabScenariosManager.TabIndex = 1;
            this.tabScenariosManager.Text = "Gestion des scénarios";
            this.tabScenariosManager.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.lblScenarioName);
            this.groupBox1.Controls.Add(this.lblNbJetonDepart);
            this.groupBox1.Controls.Add(this.txtNbJetonDepart);
            this.groupBox1.Controls.Add(this.cmbScenarioManagerNbOfPlayers);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.lblNbJoueur);
            this.groupBox1.Controls.Add(this.lblAntes);
            this.groupBox1.Controls.Add(this.dgvRobots);
            this.groupBox1.Controls.Add(this.txtBigBlind);
            this.groupBox1.Controls.Add(this.txtSmallBlind);
            this.groupBox1.Controls.Add(this.txtAntes);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(19, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(956, 215);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Gestion des scénarios";
            // 
            // textBox4
            // 
            this.textBox4.Enabled = false;
            this.textBox4.Location = new System.Drawing.Point(163, 27);
            this.textBox4.MaxLength = 3;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(102, 20);
            this.textBox4.TabIndex = 28;
            // 
            // lblScenarioName
            // 
            this.lblScenarioName.AutoSize = true;
            this.lblScenarioName.Location = new System.Drawing.Point(6, 27);
            this.lblScenarioName.Name = "lblScenarioName";
            this.lblScenarioName.Size = new System.Drawing.Size(87, 13);
            this.lblScenarioName.TabIndex = 27;
            this.lblScenarioName.Text = "Nom du scénario";
            // 
            // lblNbJetonDepart
            // 
            this.lblNbJetonDepart.AutoSize = true;
            this.lblNbJetonDepart.Location = new System.Drawing.Point(6, 163);
            this.lblNbJetonDepart.Name = "lblNbJetonDepart";
            this.lblNbJetonDepart.Size = new System.Drawing.Size(138, 13);
            this.lblNbJetonDepart.TabIndex = 9;
            this.lblNbJetonDepart.Text = "Nombre de jetons au départ";
            // 
            // txtNbJetonDepart
            // 
            this.txtNbJetonDepart.Location = new System.Drawing.Point(163, 160);
            this.txtNbJetonDepart.MaxLength = 7;
            this.txtNbJetonDepart.Name = "txtNbJetonDepart";
            this.txtNbJetonDepart.Size = new System.Drawing.Size(102, 20);
            this.txtNbJetonDepart.TabIndex = 5;
            this.txtNbJetonDepart.Text = "100";
            // 
            // cmbScenarioManagerNbOfPlayers
            // 
            this.cmbScenarioManagerNbOfPlayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScenarioManagerNbOfPlayers.Enabled = false;
            this.cmbScenarioManagerNbOfPlayers.FormattingEnabled = true;
            this.cmbScenarioManagerNbOfPlayers.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cmbScenarioManagerNbOfPlayers.Location = new System.Drawing.Point(163, 52);
            this.cmbScenarioManagerNbOfPlayers.Name = "cmbScenarioManagerNbOfPlayers";
            this.cmbScenarioManagerNbOfPlayers.Size = new System.Drawing.Size(102, 21);
            this.cmbScenarioManagerNbOfPlayers.Sorted = true;
            this.cmbScenarioManagerNbOfPlayers.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(256, 23);
            this.button1.TabIndex = 26;
            this.button1.Text = "Sauvegarder le scénario";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // lblNbJoueur
            // 
            this.lblNbJoueur.AutoSize = true;
            this.lblNbJoueur.Location = new System.Drawing.Point(6, 52);
            this.lblNbJoueur.Name = "lblNbJoueur";
            this.lblNbJoueur.Size = new System.Drawing.Size(96, 13);
            this.lblNbJoueur.TabIndex = 7;
            this.lblNbJoueur.Text = "Nombre de joueurs";
            // 
            // lblAntes
            // 
            this.lblAntes.AutoSize = true;
            this.lblAntes.Enabled = false;
            this.lblAntes.Location = new System.Drawing.Point(6, 83);
            this.lblAntes.Name = "lblAntes";
            this.lblAntes.Size = new System.Drawing.Size(34, 13);
            this.lblAntes.TabIndex = 14;
            this.lblAntes.Text = "Antes";
            // 
            // dgvRobots
            // 
            this.dgvRobots.AllowUserToAddRows = false;
            this.dgvRobots.AllowUserToDeleteRows = false;
            this.dgvRobots.AllowUserToResizeColumns = false;
            this.dgvRobots.AllowUserToResizeRows = false;
            this.dgvRobots.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRobots.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colIsRobot,
            this.colRobotType,
            this.colDescription});
            this.dgvRobots.ContextMenuStrip = this.cmsPlayers;
            this.dgvRobots.Location = new System.Drawing.Point(271, 27);
            this.dgvRobots.Name = "dgvRobots";
            this.dgvRobots.ReadOnly = true;
            this.dgvRobots.RowHeadersVisible = false;
            this.dgvRobots.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvRobots.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvRobots.Size = new System.Drawing.Size(679, 182);
            this.dgvRobots.TabIndex = 0;
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colName.DefaultCellStyle = dataGridViewCellStyle2;
            this.colName.HeaderText = "Nom";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 54;
            // 
            // colIsRobot
            // 
            this.colIsRobot.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colIsRobot.HeaderText = "Est un robot";
            this.colIsRobot.Name = "colIsRobot";
            this.colIsRobot.ReadOnly = true;
            this.colIsRobot.Width = 89;
            // 
            // colRobotType
            // 
            this.colRobotType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colRobotType.HeaderText = "Type robot";
            this.colRobotType.Name = "colRobotType";
            this.colRobotType.ReadOnly = true;
            this.colRobotType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colRobotType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colRobotType.Width = 64;
            // 
            // colDescription
            // 
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // txtBigBlind
            // 
            this.txtBigBlind.Location = new System.Drawing.Point(163, 133);
            this.txtBigBlind.MaxLength = 3;
            this.txtBigBlind.Name = "txtBigBlind";
            this.txtBigBlind.Size = new System.Drawing.Size(102, 20);
            this.txtBigBlind.TabIndex = 24;
            this.txtBigBlind.Text = "1";
            // 
            // txtSmallBlind
            // 
            this.txtSmallBlind.Location = new System.Drawing.Point(163, 109);
            this.txtSmallBlind.MaxLength = 3;
            this.txtSmallBlind.Name = "txtSmallBlind";
            this.txtSmallBlind.Size = new System.Drawing.Size(102, 20);
            this.txtSmallBlind.TabIndex = 23;
            this.txtSmallBlind.Text = "0.50";
            // 
            // txtAntes
            // 
            this.txtAntes.Enabled = false;
            this.txtAntes.Location = new System.Drawing.Point(163, 83);
            this.txtAntes.MaxLength = 3;
            this.txtAntes.Name = "txtAntes";
            this.txtAntes.Size = new System.Drawing.Size(102, 20);
            this.txtAntes.TabIndex = 20;
            this.txtAntes.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Big blind";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Small blind";
            // 
            // frmCreerPartie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 281);
            this.Controls.Add(this.tabSimulator);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmCreerPartie";
            this.ShowIcon = false;
            this.Text = "Amigo 0.1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmCreerPartie_FormClosed);
            this.grpResult.ResumeLayout(false);
            this.grpResult.PerformLayout();
            this.grpSimulation.ResumeLayout(false);
            this.grpSimulation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.cmsPlayers.ResumeLayout(false);
            this.tabSimulator.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabScenariosManager.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRobots)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpResult;
        private System.Windows.Forms.Button btnSimuler;
        private System.Windows.Forms.Label lblNbrOfHandsHeader;
        private System.Windows.Forms.GroupBox grpSimulation;
        private System.Windows.Forms.ContextMenuStrip cmsPlayers;
        private System.Windows.Forms.ToolStripMenuItem tsmiAddPlayer;
        private System.Windows.Forms.TabControl tabSimulator;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabScenariosManager;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblNbJetonDepart;
        private System.Windows.Forms.TextBox txtNbJetonDepart;
        private System.Windows.Forms.ComboBox cmbScenarioManagerNbOfPlayers;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblNbJoueur;
        private System.Windows.Forms.Label lblAntes;
        private System.Windows.Forms.DataGridView dgvRobots;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIsRobot;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRobotType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.TextBox txtBigBlind;
        private System.Windows.Forms.TextBox txtSmallBlind;
        private System.Windows.Forms.TextBox txtAntes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSimulatorNbOfPlayers;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dvtcJoueur1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dvtcJoueur2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dvtcStartingChips;
        private System.Windows.Forms.DataGridViewTextBoxColumn dvtcResetEverytime;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label lblScenarioName;
        private System.Windows.Forms.Label lblShowInterface;
        private System.Windows.Forms.RadioButton rbNo;
        private System.Windows.Forms.RadioButton rbYes;
        private System.Windows.Forms.Label lblNbrOfPotsWonHeader;
        private System.Windows.Forms.Label lblNbrOfProfitHeader;
        private System.Windows.Forms.Label lblNbrOfPotsWon;
        private System.Windows.Forms.Label lblNbrOfProfit;
        private System.Windows.Forms.Label lblNbrOfHands;
        private System.Windows.Forms.Label lblWinningPlayer;
    }
}