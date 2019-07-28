namespace Recorder
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
            this.cmd_Stop = new System.Windows.Forms.Button();
            this.cmd_Start = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmd_Stop
            // 
            this.cmd_Stop.BackColor = System.Drawing.Color.Red;
            this.cmd_Stop.Location = new System.Drawing.Point(132, 12);
            this.cmd_Stop.Name = "cmd_Stop";
            this.cmd_Stop.Size = new System.Drawing.Size(114, 42);
            this.cmd_Stop.TabIndex = 1;
            this.cmd_Stop.Text = "Stop";
            this.cmd_Stop.UseVisualStyleBackColor = false;
            this.cmd_Stop.Click += new System.EventHandler(this.cmd_Stop_Click);
            // 
            // cmd_Start
            // 
            this.cmd_Start.BackColor = System.Drawing.Color.Lime;
            this.cmd_Start.Location = new System.Drawing.Point(12, 12);
            this.cmd_Start.Name = "cmd_Start";
            this.cmd_Start.Size = new System.Drawing.Size(114, 42);
            this.cmd_Start.TabIndex = 2;
            this.cmd_Start.Text = "Start";
            this.cmd_Start.UseVisualStyleBackColor = false;
            this.cmd_Start.Click += new System.EventHandler(this.cmd_Start_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(257, 65);
            this.Controls.Add(this.cmd_Start);
            this.Controls.Add(this.cmd_Stop);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmd_Stop;
        private System.Windows.Forms.Button cmd_Start;
    }
}

