namespace BotImageHelper
{
    partial class Form1
    {
        /// <summary>
        /// Required deTYPEer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form DeTYPEer generated code

        /// <summary>
        /// Required method for DeTYPEer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.img_box = new System.Windows.Forms.PictureBox();
            this.grp_Dimension = new System.Windows.Forms.GroupBox();
            this.btn_5050 = new System.Windows.Forms.Button();
            this.btn_2020 = new System.Windows.Forms.Button();
            this.nud_Height = new System.Windows.Forms.NumericUpDown();
            this.nud_Width = new System.Windows.Forms.NumericUpDown();
            this.lbl_Height = new System.Windows.Forms.Label();
            this.lbl_Width = new System.Windows.Forms.Label();
            this.grp_Coord = new System.Windows.Forms.GroupBox();
            this.btn_CoordSelector = new System.Windows.Forms.Button();
            this.nud_Y = new System.Windows.Forms.NumericUpDown();
            this.nud_X = new System.Windows.Forms.NumericUpDown();
            this.lbl_Y = new System.Windows.Forms.Label();
            this.lbl_X = new System.Windows.Forms.Label();
            this.tgl_ZoomMode = new System.Windows.Forms.Button();
            this.btn_PrevZipImg = new System.Windows.Forms.Button();
            this.btn_NextZipImg = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.drp_CreateNewFileConstant = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmd_CreateNewFileConstant = new System.Windows.Forms.Button();
            this.lbl_LoadConstantFile = new System.Windows.Forms.Label();
            this.cmd_LoadConstantFile = new System.Windows.Forms.Button();
            this.txt_LoadConstantFile = new System.Windows.Forms.TextBox();
            this.lst_Coords = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.cmd_extract = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmd_openFolder = new System.Windows.Forms.Button();
            this.txt_destination = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmd_openFile = new System.Windows.Forms.Button();
            this.txt_source = new System.Windows.Forms.TextBox();
            this.cmd_SaveRegion = new System.Windows.Forms.Button();
            this.cmd_DeleteRegion = new System.Windows.Forms.Button();
            this.cmd_AddRegion = new System.Windows.Forms.Button();
            this.lbl_RegionName = new System.Windows.Forms.Label();
            this.txt_RegionName = new System.Windows.Forms.TextBox();
            this.txt_platform = new System.Windows.Forms.TextBox();
            this.txt_gameType = new System.Windows.Forms.TextBox();
            this.lbl_loadedImage = new System.Windows.Forms.Label();
            this.txt_Preview = new System.Windows.Forms.TextBox();
            this.cmb_FileType = new System.Windows.Forms.ComboBox();
            this.cmd_nextReference = new System.Windows.Forms.Button();
            this.cmd_PreviousReference = new System.Windows.Forms.Button();
            this.cmd_zoomReference = new System.Windows.Forms.Button();
            this.img_boxRef = new System.Windows.Forms.PictureBox();
            this.grp_References = new System.Windows.Forms.GroupBox();
            this.lbl_loadReference = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.txt_loadReference = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_differences = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.img_box)).BeginInit();
            this.grp_Dimension.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Height)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Width)).BeginInit();
            this.grp_Coord.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_X)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.img_boxRef)).BeginInit();
            this.grp_References.SuspendLayout();
            this.SuspendLayout();
            // 
            // img_box
            // 
            this.img_box.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.img_box.Location = new System.Drawing.Point(350, 12);
            this.img_box.Name = "img_box";
            this.img_box.Size = new System.Drawing.Size(200, 200);
            this.img_box.TabIndex = 15;
            this.img_box.TabStop = false;
            this.img_box.Paint += new System.Windows.Forms.PaintEventHandler(this.img_box_Paint);
            // 
            // grp_Dimension
            // 
            this.grp_Dimension.Controls.Add(this.btn_5050);
            this.grp_Dimension.Controls.Add(this.btn_2020);
            this.grp_Dimension.Controls.Add(this.nud_Height);
            this.grp_Dimension.Controls.Add(this.nud_Width);
            this.grp_Dimension.Controls.Add(this.lbl_Height);
            this.grp_Dimension.Controls.Add(this.lbl_Width);
            this.grp_Dimension.Enabled = false;
            this.grp_Dimension.Location = new System.Drawing.Point(556, 118);
            this.grp_Dimension.Name = "grp_Dimension";
            this.grp_Dimension.Size = new System.Drawing.Size(156, 100);
            this.grp_Dimension.TabIndex = 23;
            this.grp_Dimension.TabStop = false;
            this.grp_Dimension.Text = "Dimension";
            // 
            // btn_5050
            // 
            this.btn_5050.Location = new System.Drawing.Point(87, 71);
            this.btn_5050.Name = "btn_5050";
            this.btn_5050.Size = new System.Drawing.Size(63, 23);
            this.btn_5050.TabIndex = 32;
            this.btn_5050.Text = "50/50";
            this.btn_5050.UseVisualStyleBackColor = true;
            this.btn_5050.Click += new System.EventHandler(this.btn_5050_Click);
            // 
            // btn_2020
            // 
            this.btn_2020.Location = new System.Drawing.Point(6, 71);
            this.btn_2020.Name = "btn_2020";
            this.btn_2020.Size = new System.Drawing.Size(63, 23);
            this.btn_2020.TabIndex = 31;
            this.btn_2020.Text = "20/20";
            this.btn_2020.UseVisualStyleBackColor = true;
            this.btn_2020.Click += new System.EventHandler(this.btn_2020_Click);
            // 
            // nud_Height
            // 
            this.nud_Height.Location = new System.Drawing.Point(87, 45);
            this.nud_Height.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_Height.Name = "nud_Height";
            this.nud_Height.Size = new System.Drawing.Size(63, 20);
            this.nud_Height.TabIndex = 30;
            this.nud_Height.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_Height.ValueChanged += new System.EventHandler(this.nud_Height_ValueChanged);
            // 
            // nud_Width
            // 
            this.nud_Width.Location = new System.Drawing.Point(6, 45);
            this.nud_Width.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nud_Width.Name = "nud_Width";
            this.nud_Width.Size = new System.Drawing.Size(63, 20);
            this.nud_Width.TabIndex = 29;
            this.nud_Width.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nud_Width.ValueChanged += new System.EventHandler(this.nud_Width_ValueChanged);
            // 
            // lbl_Height
            // 
            this.lbl_Height.AutoSize = true;
            this.lbl_Height.Location = new System.Drawing.Point(97, 29);
            this.lbl_Height.Name = "lbl_Height";
            this.lbl_Height.Size = new System.Drawing.Size(38, 13);
            this.lbl_Height.TabIndex = 27;
            this.lbl_Height.Text = "Height";
            // 
            // lbl_Width
            // 
            this.lbl_Width.AutoSize = true;
            this.lbl_Width.Location = new System.Drawing.Point(16, 29);
            this.lbl_Width.Name = "lbl_Width";
            this.lbl_Width.Size = new System.Drawing.Size(35, 13);
            this.lbl_Width.TabIndex = 26;
            this.lbl_Width.Text = "Width";
            // 
            // grp_Coord
            // 
            this.grp_Coord.Controls.Add(this.btn_CoordSelector);
            this.grp_Coord.Controls.Add(this.nud_Y);
            this.grp_Coord.Controls.Add(this.nud_X);
            this.grp_Coord.Controls.Add(this.lbl_Y);
            this.grp_Coord.Controls.Add(this.lbl_X);
            this.grp_Coord.Enabled = false;
            this.grp_Coord.Location = new System.Drawing.Point(556, 12);
            this.grp_Coord.Name = "grp_Coord";
            this.grp_Coord.Size = new System.Drawing.Size(156, 100);
            this.grp_Coord.TabIndex = 24;
            this.grp_Coord.TabStop = false;
            this.grp_Coord.Text = "Coord";
            // 
            // btn_CoordSelector
            // 
            this.btn_CoordSelector.Location = new System.Drawing.Point(6, 71);
            this.btn_CoordSelector.Name = "btn_CoordSelector";
            this.btn_CoordSelector.Size = new System.Drawing.Size(144, 23);
            this.btn_CoordSelector.TabIndex = 30;
            this.btn_CoordSelector.Text = "CoordSelector";
            this.btn_CoordSelector.UseVisualStyleBackColor = true;
            this.btn_CoordSelector.Click += new System.EventHandler(this.btn_CoordSelector_Click);
            // 
            // nud_Y
            // 
            this.nud_Y.Location = new System.Drawing.Point(90, 45);
            this.nud_Y.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_Y.Name = "nud_Y";
            this.nud_Y.Size = new System.Drawing.Size(60, 20);
            this.nud_Y.TabIndex = 29;
            this.nud_Y.ValueChanged += new System.EventHandler(this.nud_Y_ValueChanged);
            // 
            // nud_X
            // 
            this.nud_X.Location = new System.Drawing.Point(6, 45);
            this.nud_X.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nud_X.Name = "nud_X";
            this.nud_X.Size = new System.Drawing.Size(64, 20);
            this.nud_X.TabIndex = 28;
            this.nud_X.ValueChanged += new System.EventHandler(this.nud_X_ValueChanged);
            // 
            // lbl_Y
            // 
            this.lbl_Y.AutoSize = true;
            this.lbl_Y.Location = new System.Drawing.Point(104, 29);
            this.lbl_Y.Name = "lbl_Y";
            this.lbl_Y.Size = new System.Drawing.Size(14, 13);
            this.lbl_Y.TabIndex = 27;
            this.lbl_Y.Text = "Y";
            // 
            // lbl_X
            // 
            this.lbl_X.AutoSize = true;
            this.lbl_X.Location = new System.Drawing.Point(16, 29);
            this.lbl_X.Name = "lbl_X";
            this.lbl_X.Size = new System.Drawing.Size(14, 13);
            this.lbl_X.TabIndex = 26;
            this.lbl_X.Text = "X";
            // 
            // tgl_ZoomMode
            // 
            this.tgl_ZoomMode.Enabled = false;
            this.tgl_ZoomMode.Location = new System.Drawing.Point(387, 218);
            this.tgl_ZoomMode.Name = "tgl_ZoomMode";
            this.tgl_ZoomMode.Size = new System.Drawing.Size(126, 23);
            this.tgl_ZoomMode.TabIndex = 25;
            this.tgl_ZoomMode.Text = "ToggleZoomMode";
            this.tgl_ZoomMode.UseVisualStyleBackColor = true;
            this.tgl_ZoomMode.Click += new System.EventHandler(this.tgl_StretchMode_Click);
            // 
            // btn_PrevZipImg
            // 
            this.btn_PrevZipImg.Enabled = false;
            this.btn_PrevZipImg.Location = new System.Drawing.Point(350, 218);
            this.btn_PrevZipImg.Name = "btn_PrevZipImg";
            this.btn_PrevZipImg.Size = new System.Drawing.Size(31, 23);
            this.btn_PrevZipImg.TabIndex = 26;
            this.btn_PrevZipImg.Text = "<<";
            this.btn_PrevZipImg.UseVisualStyleBackColor = true;
            this.btn_PrevZipImg.Click += new System.EventHandler(this.btn_PrevZipImg_Click);
            // 
            // btn_NextZipImg
            // 
            this.btn_NextZipImg.Enabled = false;
            this.btn_NextZipImg.Location = new System.Drawing.Point(519, 218);
            this.btn_NextZipImg.Name = "btn_NextZipImg";
            this.btn_NextZipImg.Size = new System.Drawing.Size(31, 23);
            this.btn_NextZipImg.TabIndex = 27;
            this.btn_NextZipImg.Text = ">>";
            this.btn_NextZipImg.UseVisualStyleBackColor = true;
            this.btn_NextZipImg.Click += new System.EventHandler(this.btn_NextZipImg_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.drp_CreateNewFileConstant);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.cmd_CreateNewFileConstant);
            this.groupBox1.Controls.Add(this.lbl_LoadConstantFile);
            this.groupBox1.Controls.Add(this.cmd_LoadConstantFile);
            this.groupBox1.Controls.Add(this.txt_LoadConstantFile);
            this.groupBox1.Location = new System.Drawing.Point(5, 325);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(320, 114);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ConstantsFileManager";
            // 
            // drp_CreateNewFileConstant
            // 
            this.drp_CreateNewFileConstant.FormattingEnabled = true;
            this.drp_CreateNewFileConstant.Location = new System.Drawing.Point(6, 87);
            this.drp_CreateNewFileConstant.Name = "drp_CreateNewFileConstant";
            this.drp_CreateNewFileConstant.Size = new System.Drawing.Size(219, 21);
            this.drp_CreateNewFileConstant.TabIndex = 35;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Create New";
            // 
            // cmd_CreateNewFileConstant
            // 
            this.cmd_CreateNewFileConstant.Location = new System.Drawing.Point(231, 88);
            this.cmd_CreateNewFileConstant.Name = "cmd_CreateNewFileConstant";
            this.cmd_CreateNewFileConstant.Size = new System.Drawing.Size(75, 20);
            this.cmd_CreateNewFileConstant.TabIndex = 25;
            this.cmd_CreateNewFileConstant.Text = "Create";
            this.cmd_CreateNewFileConstant.UseVisualStyleBackColor = true;
            this.cmd_CreateNewFileConstant.Click += new System.EventHandler(this.cmd_CreateNewFileConstant_Click);
            // 
            // lbl_LoadConstantFile
            // 
            this.lbl_LoadConstantFile.AutoSize = true;
            this.lbl_LoadConstantFile.Location = new System.Drawing.Point(23, 25);
            this.lbl_LoadConstantFile.Name = "lbl_LoadConstantFile";
            this.lbl_LoadConstantFile.Size = new System.Drawing.Size(47, 13);
            this.lbl_LoadConstantFile.TabIndex = 23;
            this.lbl_LoadConstantFile.Text = "LoadFile";
            // 
            // cmd_LoadConstantFile
            // 
            this.cmd_LoadConstantFile.Location = new System.Drawing.Point(258, 41);
            this.cmd_LoadConstantFile.Name = "cmd_LoadConstantFile";
            this.cmd_LoadConstantFile.Size = new System.Drawing.Size(43, 20);
            this.cmd_LoadConstantFile.TabIndex = 22;
            this.cmd_LoadConstantFile.Text = "...";
            this.cmd_LoadConstantFile.UseVisualStyleBackColor = true;
            this.cmd_LoadConstantFile.Click += new System.EventHandler(this.cmd_LoadConstantFile_Click);
            // 
            // txt_LoadConstantFile
            // 
            this.txt_LoadConstantFile.Enabled = false;
            this.txt_LoadConstantFile.Location = new System.Drawing.Point(6, 41);
            this.txt_LoadConstantFile.Name = "txt_LoadConstantFile";
            this.txt_LoadConstantFile.Size = new System.Drawing.Size(246, 20);
            this.txt_LoadConstantFile.TabIndex = 21;
            // 
            // lst_Coords
            // 
            this.lst_Coords.FormattingEnabled = true;
            this.lst_Coords.Location = new System.Drawing.Point(88, 12);
            this.lst_Coords.Name = "lst_Coords";
            this.lst_Coords.ScrollAlwaysVisible = true;
            this.lst_Coords.Size = new System.Drawing.Size(237, 199);
            this.lst_Coords.Sorted = true;
            this.lst_Coords.TabIndex = 25;
            this.lst_Coords.SelectedIndexChanged += new System.EventHandler(this.lst_Coords_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txt_name);
            this.groupBox2.Controls.Add(this.cmd_extract);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.cmd_openFolder);
            this.groupBox2.Controls.Add(this.txt_destination);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cmd_openFile);
            this.groupBox2.Controls.Add(this.txt_source);
            this.groupBox2.Location = new System.Drawing.Point(331, 273);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(325, 166);
            this.groupBox2.TabIndex = 29;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "CropImageManager";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Name";
            // 
            // txt_name
            // 
            this.txt_name.Location = new System.Drawing.Point(19, 140);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(214, 20);
            this.txt_name.TabIndex = 31;
            // 
            // cmd_extract
            // 
            this.cmd_extract.Location = new System.Drawing.Point(239, 140);
            this.cmd_extract.Name = "cmd_extract";
            this.cmd_extract.Size = new System.Drawing.Size(75, 20);
            this.cmd_extract.TabIndex = 30;
            this.cmd_extract.Text = "Extract";
            this.cmd_extract.UseVisualStyleBackColor = true;
            this.cmd_extract.Click += new System.EventHandler(this.cmd_extract_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Destination";
            // 
            // cmd_openFolder
            // 
            this.cmd_openFolder.Location = new System.Drawing.Point(271, 36);
            this.cmd_openFolder.Name = "cmd_openFolder";
            this.cmd_openFolder.Size = new System.Drawing.Size(43, 20);
            this.cmd_openFolder.TabIndex = 28;
            this.cmd_openFolder.Text = "...";
            this.cmd_openFolder.UseVisualStyleBackColor = true;
            this.cmd_openFolder.Click += new System.EventHandler(this.cmd_openFolder_Click);
            // 
            // txt_destination
            // 
            this.txt_destination.Location = new System.Drawing.Point(19, 36);
            this.txt_destination.Name = "txt_destination";
            this.txt_destination.Size = new System.Drawing.Size(246, 20);
            this.txt_destination.TabIndex = 27;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Source";
            // 
            // cmd_openFile
            // 
            this.cmd_openFile.Location = new System.Drawing.Point(271, 93);
            this.cmd_openFile.Name = "cmd_openFile";
            this.cmd_openFile.Size = new System.Drawing.Size(43, 20);
            this.cmd_openFile.TabIndex = 25;
            this.cmd_openFile.Text = "...";
            this.cmd_openFile.UseVisualStyleBackColor = true;
            this.cmd_openFile.Click += new System.EventHandler(this.cmd_openFile_Click);
            // 
            // txt_source
            // 
            this.txt_source.Enabled = false;
            this.txt_source.Location = new System.Drawing.Point(19, 93);
            this.txt_source.Name = "txt_source";
            this.txt_source.Size = new System.Drawing.Size(246, 20);
            this.txt_source.TabIndex = 24;
            // 
            // cmd_SaveRegion
            // 
            this.cmd_SaveRegion.BackColor = System.Drawing.Color.Fuchsia;
            this.cmd_SaveRegion.Enabled = false;
            this.cmd_SaveRegion.Location = new System.Drawing.Point(171, 220);
            this.cmd_SaveRegion.Name = "cmd_SaveRegion";
            this.cmd_SaveRegion.Size = new System.Drawing.Size(73, 23);
            this.cmd_SaveRegion.TabIndex = 30;
            this.cmd_SaveRegion.Text = "Save";
            this.cmd_SaveRegion.UseVisualStyleBackColor = false;
            this.cmd_SaveRegion.Click += new System.EventHandler(this.cmd_SaveRegion_Click);
            // 
            // cmd_DeleteRegion
            // 
            this.cmd_DeleteRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.cmd_DeleteRegion.Enabled = false;
            this.cmd_DeleteRegion.Location = new System.Drawing.Point(250, 220);
            this.cmd_DeleteRegion.Name = "cmd_DeleteRegion";
            this.cmd_DeleteRegion.Size = new System.Drawing.Size(75, 23);
            this.cmd_DeleteRegion.TabIndex = 31;
            this.cmd_DeleteRegion.Text = "Delete";
            this.cmd_DeleteRegion.UseVisualStyleBackColor = false;
            this.cmd_DeleteRegion.Click += new System.EventHandler(this.cmd_DeleteRegion_Click);
            // 
            // cmd_AddRegion
            // 
            this.cmd_AddRegion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.cmd_AddRegion.Enabled = false;
            this.cmd_AddRegion.Location = new System.Drawing.Point(88, 220);
            this.cmd_AddRegion.Name = "cmd_AddRegion";
            this.cmd_AddRegion.Size = new System.Drawing.Size(77, 23);
            this.cmd_AddRegion.TabIndex = 32;
            this.cmd_AddRegion.Text = "Add";
            this.cmd_AddRegion.UseVisualStyleBackColor = false;
            this.cmd_AddRegion.Click += new System.EventHandler(this.cmd_AddRegion_Click);
            // 
            // lbl_RegionName
            // 
            this.lbl_RegionName.AutoSize = true;
            this.lbl_RegionName.Location = new System.Drawing.Point(10, 249);
            this.lbl_RegionName.Name = "lbl_RegionName";
            this.lbl_RegionName.Size = new System.Drawing.Size(72, 13);
            this.lbl_RegionName.TabIndex = 34;
            this.lbl_RegionName.Text = "Region Name";
            // 
            // txt_RegionName
            // 
            this.txt_RegionName.Enabled = false;
            this.txt_RegionName.Location = new System.Drawing.Point(88, 247);
            this.txt_RegionName.Name = "txt_RegionName";
            this.txt_RegionName.Size = new System.Drawing.Size(237, 20);
            this.txt_RegionName.TabIndex = 33;
            // 
            // txt_platform
            // 
            this.txt_platform.Location = new System.Drawing.Point(88, 299);
            this.txt_platform.Name = "txt_platform";
            this.txt_platform.Size = new System.Drawing.Size(237, 20);
            this.txt_platform.TabIndex = 35;
            this.txt_platform.Text = "ESPACEJEUX";
            // 
            // txt_gameType
            // 
            this.txt_gameType.Location = new System.Drawing.Point(88, 273);
            this.txt_gameType.Name = "txt_gameType";
            this.txt_gameType.Size = new System.Drawing.Size(237, 20);
            this.txt_gameType.TabIndex = 36;
            this.txt_gameType.Text = "TWOMAX";
            // 
            // lbl_loadedImage
            // 
            this.lbl_loadedImage.AutoSize = true;
            this.lbl_loadedImage.Location = new System.Drawing.Point(22, 35);
            this.lbl_loadedImage.Name = "lbl_loadedImage";
            this.lbl_loadedImage.Size = new System.Drawing.Size(0, 13);
            this.lbl_loadedImage.TabIndex = 37;
            // 
            // txt_Preview
            // 
            this.txt_Preview.Location = new System.Drawing.Point(947, 12);
            this.txt_Preview.Multiline = true;
            this.txt_Preview.Name = "txt_Preview";
            this.txt_Preview.ReadOnly = true;
            this.txt_Preview.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txt_Preview.Size = new System.Drawing.Size(403, 348);
            this.txt_Preview.TabIndex = 38;
            this.txt_Preview.WordWrap = false;
            // 
            // cmb_FileType
            // 
            this.cmb_FileType.FormattingEnabled = true;
            this.cmb_FileType.Location = new System.Drawing.Point(1035, 366);
            this.cmb_FileType.Name = "cmb_FileType";
            this.cmb_FileType.Size = new System.Drawing.Size(101, 21);
            this.cmb_FileType.TabIndex = 39;
            this.cmb_FileType.SelectedIndexChanged += new System.EventHandler(this.cmb_FileType_SelectedIndexChanged);
            // 
            // cmd_nextReference
            // 
            this.cmd_nextReference.Enabled = false;
            this.cmd_nextReference.Location = new System.Drawing.Point(887, 218);
            this.cmd_nextReference.Name = "cmd_nextReference";
            this.cmd_nextReference.Size = new System.Drawing.Size(31, 23);
            this.cmd_nextReference.TabIndex = 43;
            this.cmd_nextReference.Text = ">>";
            this.cmd_nextReference.UseVisualStyleBackColor = true;
            this.cmd_nextReference.Click += new System.EventHandler(this.cmd_nextReference_Click);
            // 
            // cmd_PreviousReference
            // 
            this.cmd_PreviousReference.Enabled = false;
            this.cmd_PreviousReference.Location = new System.Drawing.Point(718, 218);
            this.cmd_PreviousReference.Name = "cmd_PreviousReference";
            this.cmd_PreviousReference.Size = new System.Drawing.Size(31, 23);
            this.cmd_PreviousReference.TabIndex = 42;
            this.cmd_PreviousReference.Text = "<<";
            this.cmd_PreviousReference.UseVisualStyleBackColor = true;
            this.cmd_PreviousReference.Click += new System.EventHandler(this.cmd_PreviousReference_Click);
            // 
            // cmd_zoomReference
            // 
            this.cmd_zoomReference.Enabled = false;
            this.cmd_zoomReference.Location = new System.Drawing.Point(755, 218);
            this.cmd_zoomReference.Name = "cmd_zoomReference";
            this.cmd_zoomReference.Size = new System.Drawing.Size(126, 23);
            this.cmd_zoomReference.TabIndex = 41;
            this.cmd_zoomReference.Text = "ToggleZoomMode";
            this.cmd_zoomReference.UseVisualStyleBackColor = true;
            this.cmd_zoomReference.Click += new System.EventHandler(this.cmd_zoomReference_Click);
            // 
            // img_boxRef
            // 
            this.img_boxRef.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.img_boxRef.Location = new System.Drawing.Point(718, 12);
            this.img_boxRef.Name = "img_boxRef";
            this.img_boxRef.Size = new System.Drawing.Size(200, 200);
            this.img_boxRef.TabIndex = 40;
            this.img_boxRef.TabStop = false;
            this.img_boxRef.Paint += new System.Windows.Forms.PaintEventHandler(this.img_boxRef_Paint);
            // 
            // grp_References
            // 
            this.grp_References.Controls.Add(this.lbl_loadReference);
            this.grp_References.Controls.Add(this.button5);
            this.grp_References.Controls.Add(this.txt_loadReference);
            this.grp_References.Location = new System.Drawing.Point(663, 309);
            this.grp_References.Name = "grp_References";
            this.grp_References.Size = new System.Drawing.Size(255, 87);
            this.grp_References.TabIndex = 44;
            this.grp_References.TabStop = false;
            this.grp_References.Text = "ReferencesManager";
            // 
            // lbl_loadReference
            // 
            this.lbl_loadReference.AutoSize = true;
            this.lbl_loadReference.Location = new System.Drawing.Point(23, 42);
            this.lbl_loadReference.Name = "lbl_loadReference";
            this.lbl_loadReference.Size = new System.Drawing.Size(47, 13);
            this.lbl_loadReference.TabIndex = 23;
            this.lbl_loadReference.Text = "LoadFile";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(206, 58);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(43, 20);
            this.button5.TabIndex = 22;
            this.button5.Text = "...";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // txt_loadReference
            // 
            this.txt_loadReference.Enabled = false;
            this.txt_loadReference.Location = new System.Drawing.Point(6, 58);
            this.txt_loadReference.Name = "txt_loadReference";
            this.txt_loadReference.Size = new System.Drawing.Size(194, 20);
            this.txt_loadReference.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 276);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 45;
            this.label5.Text = "GameType";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(37, 301);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 13);
            this.label6.TabIndex = 46;
            this.label6.Text = "Platform";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(938, 369);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(91, 13);
            this.label7.TabIndex = 47;
            this.label7.Text = "ConstantsFormats";
            // 
            // txt_differences
            // 
            this.txt_differences.Enabled = false;
            this.txt_differences.Location = new System.Drawing.Point(556, 242);
            this.txt_differences.Name = "txt_differences";
            this.txt_differences.Size = new System.Drawing.Size(156, 20);
            this.txt_differences.TabIndex = 48;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(557, 223);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "OpenCL Differences";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1362, 451);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txt_differences);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.grp_References);
            this.Controls.Add(this.cmd_nextReference);
            this.Controls.Add(this.cmd_PreviousReference);
            this.Controls.Add(this.cmd_zoomReference);
            this.Controls.Add(this.img_boxRef);
            this.Controls.Add(this.cmb_FileType);
            this.Controls.Add(this.txt_Preview);
            this.Controls.Add(this.lbl_loadedImage);
            this.Controls.Add(this.txt_gameType);
            this.Controls.Add(this.txt_platform);
            this.Controls.Add(this.lbl_RegionName);
            this.Controls.Add(this.txt_RegionName);
            this.Controls.Add(this.cmd_AddRegion);
            this.Controls.Add(this.cmd_DeleteRegion);
            this.Controls.Add(this.cmd_SaveRegion);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.lst_Coords);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_NextZipImg);
            this.Controls.Add(this.btn_PrevZipImg);
            this.Controls.Add(this.tgl_ZoomMode);
            this.Controls.Add(this.grp_Coord);
            this.Controls.Add(this.grp_Dimension);
            this.Controls.Add(this.img_box);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.img_box)).EndInit();
            this.grp_Dimension.ResumeLayout(false);
            this.grp_Dimension.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Height)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Width)).EndInit();
            this.grp_Coord.ResumeLayout(false);
            this.grp_Coord.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_X)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.img_boxRef)).EndInit();
            this.grp_References.ResumeLayout(false);
            this.grp_References.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox img_box;
        private System.Windows.Forms.GroupBox grp_Dimension;
        private System.Windows.Forms.NumericUpDown nud_Height;
        private System.Windows.Forms.NumericUpDown nud_Width;
        private System.Windows.Forms.Label lbl_Height;
        private System.Windows.Forms.Label lbl_Width;
        private System.Windows.Forms.GroupBox grp_Coord;
        private System.Windows.Forms.NumericUpDown nud_Y;
        private System.Windows.Forms.NumericUpDown nud_X;
        private System.Windows.Forms.Label lbl_Y;
        private System.Windows.Forms.Label lbl_X;
        private System.Windows.Forms.Button btn_CoordSelector;
        private System.Windows.Forms.Button tgl_ZoomMode;
        private System.Windows.Forms.Button btn_5050;
        private System.Windows.Forms.Button btn_2020;
        private System.Windows.Forms.Button btn_PrevZipImg;
        private System.Windows.Forms.Button btn_NextZipImg;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbl_LoadConstantFile;
        private System.Windows.Forms.Button cmd_LoadConstantFile;
        private System.Windows.Forms.TextBox txt_LoadConstantFile;
        private System.Windows.Forms.ListBox lst_Coords;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Button cmd_extract;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cmd_openFolder;
        private System.Windows.Forms.TextBox txt_destination;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cmd_openFile;
        private System.Windows.Forms.TextBox txt_source;
        private System.Windows.Forms.Button cmd_SaveRegion;
        private System.Windows.Forms.Button cmd_DeleteRegion;
        private System.Windows.Forms.Button cmd_AddRegion;
        private System.Windows.Forms.Label lbl_RegionName;
        private System.Windows.Forms.TextBox txt_RegionName;
        private System.Windows.Forms.ComboBox drp_CreateNewFileConstant;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button cmd_CreateNewFileConstant;
        private System.Windows.Forms.TextBox txt_platform;
        private System.Windows.Forms.TextBox txt_gameType;
        private System.Windows.Forms.Label lbl_loadedImage;
        private System.Windows.Forms.TextBox txt_Preview;
        private System.Windows.Forms.ComboBox cmb_FileType;
        private System.Windows.Forms.Button cmd_nextReference;
        private System.Windows.Forms.Button cmd_PreviousReference;
        private System.Windows.Forms.Button cmd_zoomReference;
        private System.Windows.Forms.PictureBox img_boxRef;
        private System.Windows.Forms.GroupBox grp_References;
        private System.Windows.Forms.Label lbl_loadReference;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox txt_loadReference;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_differences;
        private System.Windows.Forms.Label label8;
    }
}

