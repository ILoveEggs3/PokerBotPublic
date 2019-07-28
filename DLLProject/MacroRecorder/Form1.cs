using AmigoDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Recorder
{
    public partial class Form1 : Form
    {
        CMacroRecorderController FFRecorder;
        public Form1()
        {
            FFRecorder = new CMacroRecorderController(CDBHelper.InsertMovement);
            InitializeComponent();
        }

        private void cmd_Start_Click(object sender, EventArgs e)
        {
            FFRecorder.Start();
            //CDBHelper.GetMouseMovements();
        }

        private void cmd_Stop_Click(object sender, EventArgs e)
        {
            FFRecorder.Stop();
        }


    }
}
