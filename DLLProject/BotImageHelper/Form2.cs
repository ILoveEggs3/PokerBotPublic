using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BotImageHelper
{
    public partial class Form2 : Form
    {
        public Point location;
        public Form2()
        {
            InitializeComponent();
        }

        public void Form2_MouseDown(object sender, MouseEventArgs e)
        {
            location = new Point(e.X, e.Y);
            this.Close();
        }
    }
}
