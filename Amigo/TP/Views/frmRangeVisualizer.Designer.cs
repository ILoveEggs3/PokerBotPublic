using Shared.Poker.Views;

namespace Amigo.Views
{
    partial class frmRangeVisualizer
    {
        /// <summary>
        /// Required designer variable.
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chartRange = new Shared.Poker.Views.PokerHandChart();
            this.SuspendLayout();
            // 
            // chartRange
            // 
            this.chartRange.CellFont = "Arial";
            this.chartRange.HeaderFont = "Verdana";
            this.chartRange.Location = new System.Drawing.Point(6, 4);
            this.chartRange.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.chartRange.Name = "chartRange";
            this.chartRange.Size = new System.Drawing.Size(644, 644);
            this.chartRange.TabIndex = 0;
            // 
            // frmRangeVisualizer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(3F, 6F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 900);
            this.Controls.Add(this.chartRange);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.MaximizeBox = false;
            this.Name = "frmRangeVisualizer";
            this.ShowIcon = false;
            this.Text = "Range visualizer";
            this.ResumeLayout(false);

        }

        #endregion

        private PokerHandChart chartRange;
    }
}