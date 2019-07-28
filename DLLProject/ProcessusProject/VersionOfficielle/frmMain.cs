using System;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace VersionOfficielle
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        public unsafe void button8_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestingClass.testOpenCLControllerCalculateDistancesFunction();
        }
    }
}
